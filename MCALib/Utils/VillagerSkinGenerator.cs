using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using System.IO.Compression;
using MCToolsCommonLib.Common;
using MCToolsCommonLib.Util;
using MCALib.Data;
using MCToolsCommonLib.Utils;

namespace MCALib.Utils
{
    /// <summary>
    /// MCAの村人のスキンを生成するためのクラス
    /// </summary>
    public class VillagerSkinGenerator : IDisposable
    {
        /// <summary>
        /// ゲームディレクトリのパス
        /// </summary>
        private string _gameDirectory = "";

        /// <summary>
        /// MODコレクター
        /// </summary>
        private ModCollector _modCollector;

        /// <summary>
        /// JARローダー
        /// </summary>
        private JarLoader _jarLoader;

        /// <summary>
        /// リソースパック情報
        /// </summary>
        private ResourcePackInfo _resourcePack;

        /// <summary>
        /// スキンの色データ
        /// </summary>
        private Mat<Vec4b> _skinColor;

        /// <summary>
        /// 髪の色データ
        /// </summary>
        private Mat<Vec4b> _hairColor;

        /// <summary>
        /// 破棄済みフラグ
        /// </summary>
        private bool _disposed = false;

        public VillagerSkinGenerator()
        {
            _gameDirectory = "";
            _modCollector = new ModCollector();
            _jarLoader = new JarLoader();
            _resourcePack = new ResourcePackInfo("");
            _skinColor = new Mat<Vec4b>();
            _hairColor = new Mat<Vec4b>();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="gameDirectory">ゲームディレクトリのパス</param>
        public VillagerSkinGenerator(string gameDirectory)
        {
            _gameDirectory = gameDirectory;
            _modCollector = new ModCollector(Path.Combine(_gameDirectory, "mods"), "ja-jp");

            // MCA Rebornがない場合は例外を投げる
            string mcaModPath = _modCollector.GetModPath("mca");
            if (mcaModPath == "")
            {
                throw new FileNotFoundException("MCA Rebornが見つかりません。");
            }

            _jarLoader = new JarLoader(mcaModPath, "ja-jp");

            _resourcePack = new ResourcePackInfo(Path.Combine(_gameDirectory, "resourcepacks"));
            _resourcePack.GetList();

            _skinColor = _jarLoader.LoadImage("assets/mca/textures/colormap/villager_skin.png");
            _hairColor = _jarLoader.LoadImage("assets/mca/textures/colormap/villager_hair.png");
        }

        /// <summary>
        /// Disposeメソッド
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed == false)
            {
                _disposed = true;
                if (_skinColor != null)
                {
                    _skinColor.Dispose();
                }

                if (_hairColor != null)
                {
                    _hairColor.Dispose();
                }

                if (disposing == true)
                {
                    GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>
        /// Disposeメソッド
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~VillagerSkinGenerator()
        {
            Dispose(false);
        }

        /// <summary>
        /// スキンを生成して指定されたパスに書き出す。
        /// </summary>
        /// <param name="data">村人データ</param>
        /// <param name="outputBasePath">出力先パス</param>
        /// <param name="uuidOnly">ファイル名をUUIDのみにするかのフラグ</param>
        public void OutputSkin(in MCAVillagerInfo data, string outputBasePath, bool uuidOnly = true)
        {
            // スキン生成
            Mat<Vec4b> img = GenerateSkin(data);

            // 画像書き出し
            string skinPath = Path.Combine(outputBasePath, GenerateSkinFileName(data, uuidOnly));
            Cv2.ImWrite(skinPath, img);

            img.Dispose();
            return;
        }

        /// <summary>
        /// スキンパス生成
        /// </summary>
        /// <param name="data">村人データ</param>
        /// <param name="uuidOnly">ファイル名をUUIDのみにするかのフラグ</param>
        /// <returns>スキンパス</returns>
        public string GenerateSkinFileName(in MCAVillagerInfo data, bool uuidOnly = true)
        {
            string skinFileName = uuidOnly ? $"{data.UUID}.png" :
                $"region_{data.RegionPos.X}_{data.RegionPos.Z}_{data.UUID}.png";
            return skinFileName;
        }

        /// <summary>
        /// サムネイル(顔)の生成
        /// </summary>
        /// <param name="data">村人データ</param>
        /// <param name="size">サムネイルのサイズ</param>
        /// <returns>サムネイル</returns>
        public BitmapSource GenerateFaceThumb(in MCAVillagerInfo data, int size = 32)
        {
            BitmapSource thumb;
            using (ResourcesTracker tracker = new ResourcesTracker())
            {
                Mat<Vec4b> img = GenerateSkin(data);
                Mat<Vec4b> outImg = tracker.T(new Mat<Vec4b>(img.Clone(CommonDefine.SkinFrontPositions["head"])));
                ImageProcessing.ImageOverlay(tracker.T(new Mat<Vec4b>(img.Clone(CommonDefine.SkinFrontPositions["head_armor"]))), ref outImg);
                thumb = BitmapSourceConverter.ToBitmapSource(tracker.T(new Mat<Vec4b>(outImg.Resize(new OpenCvSharp.Size(size, size), interpolation: InterpolationFlags.Nearest))));
            }

            return thumb;
        }

        /// <summary>
        /// スキン生成
        /// </summary>
        /// <param name="data">村人データ</param>
        /// <returns>スキン</returns>
        public Mat<Vec4b> GenerateSkin(in MCAVillagerInfo data)
        {
            Mat<Vec4b> img = new Mat<Vec4b>();
            using (ResourcesTracker tracker = new ResourcesTracker())
            {
                img = new Mat<Vec4b>(GetBaseSkin(tracker, data));
                OverlayEye(tracker, data, ref img);
                OverlayHair(tracker, data, ref img);
                OverlayClothing(tracker, data, ref img);
            }

            return img;
        }

        /// <summary>
        /// リソースデータ取得
        /// </summary>
        /// <param name="tracker">リソーストラッカー</param>
        /// <param name="orgPath">元のパス</param>
        /// <returns>画像</returns>
        private Mat<Vec4b> GetTextureData(in ResourcesTracker tracker, string orgPath)
        {
            string nameSpace = orgPath.Split(':')[0];
            string path = orgPath.Replace(":", "/");
            Mat<Vec4b> img;

            // 名前空間で処理を分岐
            switch (nameSpace)
            {
                // mcaの場合
                case "mca":
                    if (_resourcePack.ExistsResource(nameSpace , path))
                    {
                        // リソースパックから取得
                        img = _resourcePack.GetTextureData(tracker, path, nameSpace);
                    }
                    else
                    {
                        // JARローダーから画像を取得
                        img = tracker.T(_jarLoader.LoadImage($"assets/{path}"));
                    }

                    break;

                // immersive_libraryの場合は、ゲームディレクトリ配下のimmersive_libraryから画像を取得
                case "immersive_library":
                    path = path.Replace("/", "\\");
                    img = tracker.T(new Mat<Vec4b>(Cv2.ImRead(Path.Combine(_gameDirectory, path + ".png"), ImreadModes.Unchanged)));
                    break;

                // その他の名前空間の場合は、リソースパックから画像を取得
                default:
                    img = _resourcePack.GetTextureData(tracker, path, nameSpace);
                    break;
            }

            return img;
        }

        /// <summary>
        /// 素体画像パス取得
        /// </summary>
        /// <param name="geneSkin">スキンID</param>
        /// <param name="gender">性別</param>
        /// <returns>素体画像</returns>
        private Mat<Vec4b> GetOriginalSkinPath(in ResourcesTracker tracker, float geneSkin, string gender)
        {
            int skinID = (int)Math.Min(4.0F, Math.Max(0.0F, geneSkin * 5));
            return GetTextureData(tracker, $"mca:skins/skin/{gender}/{skinID}.png");
        }

        /// <summary>
        /// 髪の色取得
        /// </summary>
        /// <param name="data">村人データ</param>
        /// <returns>髪の色</returns>
        private Vec4b GetHairColor(in MCAVillagerInfo data)
        {
            Vec4b color = new Vec4b();
            if ((data.HairColorBlue > 0.0F) || (data.HairColorGreen > 0.0F) || (data.HairColorRed > 0.0F))
            {
                color.Item0 = (byte)CommonLib.ClampFloor<float>(data.HairColorBlue, 255.0F);
                color.Item1 = (byte)CommonLib.ClampFloor<float>(data.HairColorGreen, 255.0F);
                color.Item2 = (byte)CommonLib.ClampFloor<float>(data.HairColorRed, 255.0F);
            }
            else
            {
                int x = (int)CommonLib.ClampFloor<float>(data.GenePheomelanin * data.Albinism, (float)(_hairColor.Width - 1));
                int y = (int)CommonLib.ClampFloor<float>(data.GeneEumelanin * data.Albinism, (float)(_hairColor.Width - 1));
                color = _hairColor.GetIndexer()[y, x];
            }

            return color;
        }

        /// <summary>
        /// 素体取得
        /// </summary>
        /// <param name="tracker">リソーストラッカー</param>
        /// <param name="data">村人データ</param>
        /// <returns>素体画像</returns>
        private Mat<Vec4b> GetBaseSkin(in ResourcesTracker tracker, in MCAVillagerInfo data)
        {
            Mat<Vec4b> img = GetOriginalSkinPath(tracker, data.GeneSkin, data.Gender);
            int x = (int)CommonLib.ClampFloor<float>(data.GeneHemoglobin * data.Albinism, (float)(_skinColor.Width - 1));
            int y = (int)CommonLib.ClampFloor<float>(data.GeneMelanin * data.Albinism, (float)(_skinColor.Height - 1));
            Vec4b color = _skinColor.GetIndexer()[y, x];
            ImageProcessing.ImageMultiply(color, ref img);
            return img;
        }

        /// <summary>
        /// 目のレンダリング
        /// </summary>
        /// <param name="tracker">リソーストラッカー</param>
        /// <param name="data">村人データ</param>
        /// <param name="img">レンダリング元の画像</param>
        private void OverlayEye(in ResourcesTracker tracker, in MCAVillagerInfo data, ref Mat<Vec4b> img)
        {
            int index = (int)Math.Min((float)(CommonDefine.MCAFaceCount - 1), Math.Max(0.0F, data.GeneFace * (float)CommonDefine.MCAFaceCount));
            string hetero = data.Hetero ? "_hetero" : "";
            Mat<Vec4b> eyeImg = GetTextureData(tracker, $"mca:skins/face/normal/{data.Gender}/{index}{hetero}.png");
            ImageProcessing.ImageOverlay(eyeImg, ref img);
            return;
        }

        /// <summary>
        /// 髪のレンダリング
        /// </summary>
        /// <param name="tracker">リソーストラッカー</param>
        /// <param name="data">村人データ</param>
        /// <param name="img">レンダリング元の画像</param>
        private void OverlayHair(in ResourcesTracker tracker, in MCAVillagerInfo data, ref Mat<Vec4b> img)
        {
            Mat<Vec4b> hairImg = GetTextureData(tracker, data.Hair);
            ImageProcessing.ImageMultiply(GetHairColor(data), ref hairImg);
            ImageProcessing.ImageOverlay(hairImg, ref img);
            return;
        }

        /// <summary>
        /// 服のレンダリング
        /// </summary>
        /// <param name="tracker">リソーストラッカー</param>
        /// <param name="data">村人データ</param>
        /// <param name="img">レンダリング元の画像</param>
        private void OverlayClothing(in ResourcesTracker tracker, in MCAVillagerInfo data, ref Mat<Vec4b> img)
        {
            ImageProcessing.ImageOverlay(GetTextureData(tracker, data.Clothes), ref img);
            return;
        }
    }
}
