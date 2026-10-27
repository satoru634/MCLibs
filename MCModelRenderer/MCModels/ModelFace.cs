using MCToolsCommonLib.Common;
using MCToolsCommonLib.Utils;
using OpenCvSharp;

namespace MCModelRenderer.MCModels
{
    /// <summary>
    /// モデルの面を表すクラス。
    /// </summary>
    public class ModelFace : IDisposable
    {
        /// <summary>
        /// UV矩形座標。
        /// </summary>
        public Rect UV { get; set; }

        /// <summary>
        /// 回転角度。
        /// </summary>
        public int Rotate { get; set; }

        /// <summary>
        /// テクスチャのパス。
        /// </summary>
        public string Texture { get; set; }

        /// <summary>
        /// テクスチャのフリップモード。
        /// </summary>
        public List<FlipMode> TextureFlip { get; set; }

        /// <summary>
        /// カリング面。
        /// </summary>
        public string Cullface { get; set; }

        /// <summary>
        /// リソースの解放状態を示すフラグ。
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// デフォルトコンストラクタ。
        /// </summary>
        public ModelFace()
        {
            UV = new Rect(new Point(0.0, 0.0), new Size(16.0, 16.0));
            Rotate = 0;
            Texture = "";
            TextureFlip = new List<FlipMode>();
            Cullface = "";
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="uv">UV矩形座標</param>
        /// <param name="rotate">回転角度</param>
        /// <param name="texture">テクスチャのパス</param>
        /// <param name="textureFlip">テクスチャのフリップモード</param>
        /// <param name="cullface">カリング面</param>
        public ModelFace(Rect uv, int rotate, string texture, List<FlipMode> textureFlip, string cullface)
        {
            UV = uv;
            Rotate = rotate;
            Texture = texture;
            TextureFlip = textureFlip;
            Cullface = cullface;
        }

        /// <summary>
        /// ModelFaceのコンストラクタ。
        /// </summary>
        /// <param name="orgFace">オリジナルのFaceデータ(JSON形式)</param>
        public ModelFace(object orgFace)
        {
            UV = new Rect(new Point(0, 0), new Size(16, 16));
            Rotate = 0;
            Texture = "";
            TextureFlip = new List<FlipMode>();
            Cullface = "";

            string? strFace = orgFace.ToString();
            if (strFace == null)
            {
                return;
            }

            var face = CommonLib.DeserializeJson<Dictionary<string, object>>(strFace);
            foreach (var pair in face)
            {
                switch (pair.Key)
                {
                    // UV座標
                    case "uv":
                        UV = InitUV(pair.Value);
                        break;

                    // 回転角度
                    case "rotation":
                        Rotate = ConvertJsonValue.ConvertInt(pair.Value);
                        break;

                    // テクスチャのパス
                    case "texture":
                        Texture = ConvertJsonValue.ConvertStr(pair.Value);
                        break;

                    // カリング面
                    case "cullface":
                        Cullface = ConvertJsonValue.ConvertStr(pair.Value);
                        break;
                }
            }
        }

        /// <summary>
        /// リソースを解放するためのメソッド。
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed == false)
            {
                _disposed = true;

                TextureFlip.Clear();

                if (disposing == true)
                {
                    GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>
        /// リソースを解放するためのメソッド。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// デストラクタ。
        /// </summary>
        ~ModelFace()
        {
            Dispose(false);
        }

        /// <summary>
        /// UV座標を初期化する。
        /// </summary>
        /// <param name="orgUV">UV座標(JSON形式)</param>
        /// <returns>UV座標</returns>
        private Rect InitUV(object orgUV)
        {
            string? strUV = orgUV.ToString();
            if (strUV == null)
            {
                return new Rect(new Point(0, 0), new Size(16, 16));
            }

            var rawUV = CommonLib.DeserializeJson<List<double>>(strUV);
            if (rawUV.Count != 4)
            {
                return new Rect(new Point(0, 0), new Size(16, 16));
            }

            // UV座標の順序を確認し、必要に応じてフリップモードを追加
            double x1, x2, y1, y2;
            if (rawUV[0] < rawUV[2])
            {
                x1 = rawUV[0];
                x2 = rawUV[2];
            }
            else
            {
                x1 = rawUV[2];
                x2 = rawUV[0];
                TextureFlip.Add(FlipMode.Y);
            }

            if (rawUV[1] < rawUV[3])
            {
                y1 = rawUV[1];
                y2 = rawUV[3];
            }
            else
            {
                y1 = rawUV[3];
                y2 = rawUV[1];
                TextureFlip.Add(FlipMode.X);
            }

            return new Rect(new Point(x1, y1), new Size(Math.Abs(x2 - x1), Math.Abs(y2 - y1)));
        }
    }
}
