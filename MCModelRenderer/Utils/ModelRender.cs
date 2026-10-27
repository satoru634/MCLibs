using MCModelRenderer.MCModels;
using MCToolsCommonLib.Common;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using SharpDX;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;
using SharpDX.Direct3D11;

using MeshGeometry3D = HelixToolkit.SharpDX.Core.MeshGeometry3D;
using Color = System.Windows.Media.Color;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;
using OrthographicCamera = HelixToolkit.Wpf.SharpDX.OrthographicCamera;

namespace MCModelRenderer.Utils
{
    /// <summary>
    /// モデルをレンダリングするためのクラス
    /// </summary>
    public class ModelRender
    {
        public IEffectsManager EffectsManager { get; set; }
        public Camera ViewportCamera { get; set; }

        /// <summary>
        /// レンダリングする幅
        /// </summary>
        private int _width { get; set; }

        /// <summary>
        /// レンダリングする高さ
        /// </summary>
        private int _height { get; set; }

        /// <summary>
        /// レンダリング対象のブロックモデル
        /// </summary>
        private BlockModel _model { get; set; }

        /// <summary>
        /// 各面の頂点インデックスを定義する辞書
        /// </summary>
        private readonly Dictionary<string, int[]> _positionIndexes = new Dictionary<string, int[]>()
        {
            {"up", [4, 7, 3, 0] },      // 左上後, 右上後, 右上前, 左上前
            {"down", [1, 2, 6, 5] },    // 左下前, 右下前, 右下後, 左下後
            {"north", [7, 4, 5, 6] },   // 右上後, 左上後, 左下後, 右下後
            {"south", [0, 3, 2, 1] },   // 左上前, 右上前, 右下前, 左下前
            {"west", [4, 0, 1, 5] },    // 左上後, 左上前, 左下前, 左下後
            {"east", [3, 7, 6, 2] }     // 右上前, 右上後, 右下後, 右下前
        };

        /// <summary>
        /// ModelRenderクラスの新しいインスタンスを初期化する。
        /// </summary>
        /// <param name="width">レンダリングする幅</param>
        /// <param name="height">レンダリングする高さ</param>
        /// <param name="model">レンダリング対象のブロックモデル</param>
        public ModelRender(bool perspective)
        {
            InitViewport(perspective);

            _width = 16;
            _height = 16;
            _model = new BlockModel();
        }

        /// <summary>
        /// ModelRenderクラスの新しいインスタンスを初期化する。
        /// </summary>
        public ModelRender()
        {
            EffectsManager = new DefaultEffectsManager();
            ViewportCamera = new PerspectiveCamera();

            _width = 16;
            _height = 16;
            _model = new BlockModel();
        }

        /// <summary>
        /// レンダリングするサイズを変更する。
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Resize(int width, int height)
        {
            _width = width;
            _height = height;
            return;
        }

        /// <summary>
        /// レンダリング対象のブロックモデルを設定する。
        /// </summary>
        /// <param name="model"></param>
        public void SetModel(BlockModel model)
        {
            _model = model;
            return;
        }

        /// <summary>
        /// モデルをレンダリングし、Viewport3Dを返す。
        /// </summary>
        /// <param name="viewport3D">レンダリング対象のViewport3D</param>
        /// <param name="perspective">透視投影を使用するかどうか</param>
        /// <returns>レンダリングされたViewport3D</returns>
        public void Render(Viewport3DX viewport, string backgroundColor)
        {
            viewport.Items.Clear();

            // 背景色の設定
            viewport.BackgroundColor = ColorConverter.StringToWpfColor(backgroundColor);

            // ライト追加
            viewport.Items.Add(new DirectionalLight3D
            {
                Direction = new Vector3D(0, 10, 0),
                Color = Colors.Gray,
            });
            viewport.Items.Add(new DirectionalLight3D
            {
                Direction = new Vector3D(-1, 3, 8),
                Color = Colors.White,
            });
            viewport.Items.Add(new DirectionalLight3D
            {
                Direction = new Vector3D(0, -1, 10),
                Color = Color.FromRgb(0x60, 0x60, 0x60),
            });

            // モデル生成
            foreach (var item in Generate3DModel())
            {
                viewport.Items.Add(item);
            }

            // オフスクリーンレンダリング
            viewport.Measure(new System.Windows.Size(_width, _height));
            viewport.Arrange(new System.Windows.Rect(0, 0, _width, _height));
            viewport.UpdateLayout();
            return;
        }

        /// <summary>
        /// Viewport3Dの内容をPNG画像として出力する。
        /// </summary>
        /// <param name="viewport">出力対象のViewport3DX</param>
        /// <param name="basePath">出力先のパス</param>
        /// <returns>出力されたファイル名</returns>
        public string Output(in Viewport3DX viewport, string basePath)
        {
            // 出力先ディレクトリの作成
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            // 出力ファイル名の生成
            string outputPath = Path.Combine(basePath, $"{_model.ItemID.Replace(":", "_")}_x{_width.ToString()}.png");

            // モデルをレンダリングしてPNGファイルに出力する
            bool result;
            if (_model.ModelType == "item")
            {
                result = OutputItemImage(outputPath);
            }
            else
            {
                result = OutputBlockImage(viewport, outputPath);
            }

            return result ? Path.GetFileName(outputPath) : string.Empty;
        }

        /// <summary>
        /// ViewportCoreを初期化する。
        /// </summary>
        /// <param name="perspective"></param>
        private void InitViewport(bool perspective)
        {
            EffectsManager = new DefaultEffectsManager();
            ViewportCamera = perspective ?
                new PerspectiveCamera
                {
                    Position = new Point3D(0, 0, 10),
                    LookDirection = new Vector3D(0, 0, -1),
                    UpDirection = new Vector3D(0, 1, 0),
                    FieldOfView = 16.5
                } :
                new OrthographicCamera
                {
                    Position = new Point3D(0, 0, 10),
                    LookDirection = new Vector3D(0, 0, -1),
                    UpDirection = new Vector3D(0, 1, 0),
                    Width = 16.5
                };

            return;
        }

        /// <summary>
        /// 3Dモデルを生成する。
        /// </summary>
        /// <returns>生成されたGeometryModel3Dのリスト</returns>
        private List<Element3D> Generate3DModel()
        {
            List<Element3D> geoModels = new List<Element3D>();

            // アイテムの場合は1面だけ生成する
            if (_model.Elements.Count == 0)
            {
                var element = GenerateItemFaceElement();
                var geoModel = GenerateFaceModel(element, "south", _positionIndexes["south"]);
                if (geoModel != null)
                {
                    geoModels.Add(geoModel);
                }

                return geoModels;
            }

            // モデル要素をループして、各面を生成
            foreach (var element in _model.Elements)
            {
                foreach (var positionIndexPair in _positionIndexes)
                {
                    var geoModel = GenerateFaceModel(element, positionIndexPair.Key, positionIndexPair.Value);
                    if (geoModel != null)
                    {
                        geoModels.Add(geoModel);
                    }
                }
            }

            return geoModels;
        }

        /// <summary>
        /// アイテムの南面を生成するための要素を作成する。
        /// </summary>
        /// <returns>生成されたModelElements</returns>
        private ModelElements GenerateItemFaceElement()
        {
            // アイテムの南面を生成するための要素を作成
            Dictionary<string, ModelFace> faces = new Dictionary<string, ModelFace>();
            Regex rx = new Regex(@"layer[0-9]{1}", RegexOptions.Compiled);

            // テクスチャをループして、南面を生成
            foreach (var texture in _model.Textures)
            {
                var face = new ModelFace();

                string key = rx.IsMatch(texture.Key) ? "layer" : texture.Key;
                face.Texture = $"#{key}";

                if (!faces.ContainsKey("south"))
                {
                    faces.Add("south", face);
                }
            }

            // アイテムの南面を生成するための要素を作成
            return new ModelElements(new Point3D(0.0, 0.0, 0.0), new Point3D(16.0, 16.0, 16.0), new ModelRotation(), faces, -8.0f);
        }

        /// <summary>
        /// 指定された要素と面に基づいてGeometryModel3Dを生成する。
        /// </summary>
        /// <param name="element">モデル要素</param>
        /// <param name="faceName">面の名前</param>
        /// <param name="positionIndex">頂点インデックス</param>
        /// <returns>MeshGeometryModel3D</returns>
        private MeshGeometryModel3D? GenerateFaceModel(ModelElements element, string faceName, int[] positionIndex)
        {
            // テクスチャ生成
            var texture = GenerateTexture(element, faceName);
            if (texture == null)
            {
                return null;
            }

            // マスク画像を生成
            var maskImg = ImageProcessing.GenerateMaskImage(texture);

            // サンプラーステートの設定
            var sampler = SamplerStateDescription.Default();
            sampler.Filter = Filter.MinMagMipPoint;
            sampler.MaximumAnisotropy = 4;
            sampler.AddressU = TextureAddressMode.Wrap;
            sampler.AddressV = TextureAddressMode.Wrap;
            sampler.AddressW = TextureAddressMode.Wrap;
            sampler.ComparisonFunction = Comparison.Always;

            // マテリアルの生成
            var material = new PhongMaterial()
            {
                DiffuseColor = Color4.White,
                SpecularColor = Color4.Black,
                DiffuseMap = new TextureModel(CommonCalc.ConvertMatVec4bToColor4Array(texture), texture.Width, texture.Height),
                DiffuseAlphaMap = new TextureModel(CommonCalc.ConvertMatVec4bToColor4Array(maskImg), maskImg.Width, maskImg.Height),
                DiffuseMapSampler = sampler,
            };

            // テクスチャとマスク画像を破棄
            texture.Dispose();
            maskImg.Dispose();

            // パーツの回転を考慮して行列を計算
            var transformGroup = new Transform3DGroup();
            if (element.Rotate.Axis != "")
            {
                // パーツ単体で回転指定がある場合
                transformGroup.Children.Add(TranslateParts(element, true));
                transformGroup.Children.Add(RotationParts(element));
                transformGroup.Children.Add(TranslateParts(element));
            }

            // 描画用の回転を設定
            Point3D rotation = _model.ModelType == "item" ? new Point3D(0.0, 0.0, 0.0) : new Point3D(30.0, 135.0, 0.0);
            if (_model.Displays.ContainsKey("gui"))
            {
                rotation = _model.Displays["gui"].Rotation;
            }

            transformGroup.Children.Add(new MatrixTransform3D(CommonCalc.CalculateRotationMatrix(rotation)));

            // モデル生成
            var vertexPosList = GenerateVertexPosList(element);
            var geoModel = new MeshGeometryModel3D()
            {
                Geometry = SelectVertexPos(vertexPosList, positionIndex),
                Material = material,
                Transform = transformGroup,
                IsTransparent = true,
            };

            return geoModel;
        }

        /// <summary>
        /// モデル要素に基づいて頂点座標を生成する。
        /// </summary>
        /// <param name="element">モデル要素</param>
        /// <returns>生成された頂点座標の配列</returns>
        private Vector3[] GenerateVertexPosList(ModelElements element)
        {
            // 頂点座標初期化
            Point3D scale = new Point3D(0.625, 0.625, 0.625);
            if (_model.Displays.ContainsKey("gui"))
            {
                scale = _model.Displays["gui"].Scale;
            }

            var from = CommonCalc.TransformPos(element.From, element.MoveAmount, scale);
            var to = CommonCalc.TransformPos(element.To, element.MoveAmount, scale);

            // 頂点を定義(立方体の8つの頂点)
            var vertices = new[]
            {
                new Vector3((float)from.X, (float)to.Y, (float)to.Z),       // 0:左上前
                new Vector3((float)from.X, (float)from.Y, (float)to.Z),     // 1:左下前
                new Vector3((float)to.X, (float)from.Y, (float)to.Z),       // 2:右下前
                new Vector3((float)to.X, (float)to.Y, (float)to.Z),         // 3:右上前
                new Vector3((float)from.X, (float)to.Y, (float)from.Z),     // 4:左上後
                new Vector3((float)from.X, (float)from.Y, (float)from.Z),   // 5:左下後
                new Vector3((float)to.X, (float)from.Y, (float)from.Z),     // 6:右下後
                new Vector3((float)to.X, (float)to.Y, (float)from.Z),       // 7:右上後
            };

            return vertices;
        }

        /// <summary>
        /// 指定された頂点インデックスに基づいて頂点座標を選択する。
        /// </summary>
        /// <param name="vertexPosList">頂点座標リスト</param>
        /// <param name="triangles">選択するインデックス</param>
        /// <returns>選択された頂点座標の配列</returns>
        private MeshGeometry3D SelectVertexPos(Vector3[] vertexPosList, int[] triangles)
        {
            var builder = new MeshBuilder(true, true); // テクスチャ座標と法線を使用
            var selectVertexPoslist = triangles.Select(x => vertexPosList[x]).ToArray();
            builder.AddQuad(selectVertexPoslist[0], selectVertexPoslist[1], selectVertexPoslist[2], selectVertexPoslist[3]);
            return builder.ToMesh();
        }

        /// <summary>
        /// 指定された要素と面に基づいてテクスチャを生成する。
        /// </summary>
        /// <param name="element">モデル要素</param>
        /// <param name="faceName">面の名前</param>
        /// <param name="scale">スケール</param>
        /// <returns>生成されたテクスチャ(BitmapSource)</returns>
        private Mat<Vec4b>? GenerateTexture(ModelElements element, string faceName, int scale)
        {
            // テクスチャが存在しない場合はnullを返す
            if (!element.Faces.ContainsKey(faceName))
            {
                return null;
            }

            var texture = GetTexture(element.Faces[faceName].Texture);

            // テクスチャ切り出し
            texture = texture[element.Faces[faceName].UV];
            foreach (var flip in element.Faces[faceName].TextureFlip)
            {
                Cv2.Flip(texture, texture, flip);
            }

            // テクスチャ回転
            switch (element.Faces[faceName].Rotate)
            {
                case 90:
                    Cv2.Rotate(texture, texture, RotateFlags.Rotate90Clockwise);
                    break;

                case 180:
                    Cv2.Rotate(texture, texture, RotateFlags.Rotate180);
                    break;

                case 270:
                    Cv2.Rotate(texture, texture, RotateFlags.Rotate90Counterclockwise);
                    break;
            }

            // リサイズ
            var resized = texture.Resize(new OpenCvSharp.Size(texture.Width * scale, texture.Height * scale), interpolation: InterpolationFlags.Nearest);
            return new Mat<Vec4b>(resized);
        }

        /// <summary>
        /// 指定された要素と面に基づいてテクスチャを生成する。
        /// </summary>
        /// <param name="element">モデル要素</param>
        /// <param name="faceName">面の名前</param>
        /// <returns>生成されたテクスチャ(ImageBrush)</returns>
        private Mat<Vec4b>? GenerateTexture(ModelElements element, string faceName)
        {
            // 2の累乗で切り捨て
            int scale = 1;
            while (scale * 2 <= Math.Min(_width, _height))
            {
                scale *= 2;
            }

            scale /= 16;

            // テクスチャを生成
            return GenerateTexture(element, faceName, scale);
        }

        /// <summary>
        /// 指定されたテクスチャ名に基づいてテクスチャを取得する。
        /// </summary>
        /// <param name="baseTextureName">テクスチャ名</param>
        /// <returns>テクスチャ</returns>
        private Mat<Vec4b> GetTexture(string baseTextureName)
        {
            baseTextureName = baseTextureName.StartsWith("#") ? baseTextureName.Replace("#", "") : baseTextureName;
            if (!_model.TextureImages.ContainsKey(baseTextureName))
            {
                return GetTexture(_model.Textures[baseTextureName]);
            }

            return _model.TextureImages[baseTextureName].Clone();
        }

        /// <summary>
        /// モデル要素の回転を表すMatrixTransform3Dを生成する。
        /// </summary>
        /// <param name="element">モデル要素</param>
        /// <returns>生成されたMatrixTransform3D</returns>
        private MatrixTransform3D RotationParts(ModelElements element)
        {
            Point3D rotation = new Point3D();
            switch (element.Rotate.Axis)
            {
                case "x":
                    rotation.X = element.Rotate.Angle;
                    break;

                case "y":
                    rotation.Y = element.Rotate.Angle;
                    break;

                case "z":
                    rotation.Z = element.Rotate.Angle;
                    break;
            }

            return new MatrixTransform3D(CommonCalc.CalculateRotationMatrix(rotation));
        }

        /// <summary>
        /// モデル要素の平行移動を表すTranslateTransform3Dを生成する。
        /// </summary>
        /// <param name="element">モデル要素</param>
        /// <param name="invert">移動を反転するかどうか</param>
        /// <returns>移動量</returns>
        private TranslateTransform3D TranslateParts(ModelElements element, bool invert = false)
        {
            // スケールを取得
            Point3D scale = new Point3D(1.0, 1.0, 1.0);
            if (_model.Displays.ContainsKey("gui"))
            {
                scale = _model.Displays["gui"].Scale;
            }

            // 移動量を取得
            var translate = CommonCalc.TransformPos(element.Rotate.Origin, element.MoveAmount, scale);
            if (invert)
            {
                translate *= -1;
            }

            return new TranslateTransform3D(translate);
        }

        /// <summary>
        /// 指定されたViewport3Dの内容をPNG画像として出力する。
        /// </summary>
        /// <param name="viewport">出力対象のViewport3DX</param>
        /// <param name="outputPath">出力先パス</param>
        private bool OutputBlockImage(in Viewport3DX viewport, string outputPath)
        {
            var bitmap = viewport.RenderBitmap();
            if (bitmap == null)
            {
                return false;
            }

            // PngBitmapEncoder を作成
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(bitmap));

            // ファイルストリームを作成
            using (FileStream fileStream = new FileStream(outputPath, FileMode.Create))
            {
                // PNG 画像をファイルに保存
                pngEncoder.Save(fileStream);
            }

            return true;
        }

        /// <summary>
        /// アイテムの画像を生成し、指定されたパスに保存する。
        /// </summary>
        /// <param name="outputPath">出力先パス</param>
        private bool OutputItemImage(string outputPath)
        {
            // アイテムの画像を生成
            var element = GenerateItemFaceElement();
            var itemImage = GenerateTexture(element, "south");
            if (itemImage == null)
            {
                return false;
            }

            Cv2.ImWrite(outputPath, itemImage);
            itemImage.Dispose();
            return true;
        }

    }
}
