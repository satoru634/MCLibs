using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.Common
{
    public class ImageProcessing
    {
        /// <summary>
        /// 指定された画像をオーバーレイする。
        /// </summary>
        /// <param name="overlayImg">オーバーレイする画像</param>
        /// <param name="baseImg">元の画像</param>
        static public void ImageOverlay(in Mat<Vec4b> overlayImg, ref Mat<Vec4b> baseImg, Point drawStart = new Point())
        {
            var baseIndexer = baseImg.GetIndexer();
            var overlayIndexer = overlayImg.GetIndexer();
            for (int x = 0; x < overlayImg.Width; x++)
            {
                for (int y = 0; y < overlayImg.Height; y++)
                {
                    // アルファ値が0の場合、オーバーレイを適用しない
                    if (overlayIndexer[y, x].Item3 == 0)
                    {
                        continue;
                    }

                    // アルファ値が0でない場合、オーバーレイを適用
                    baseIndexer[y + drawStart.Y, x + drawStart.X] = new Vec4b(
                        overlayIndexer[y, x].Item0,
                        overlayIndexer[y, x].Item1,
                        overlayIndexer[y, x].Item2,
                        overlayIndexer[y, x].Item3
                    );
                }
            }

            return;
        }

        /// <summary>
        /// 指定された色で画像を乗算する。
        /// </summary>
        /// <param name="color">乗算する色</param>
        /// <param name="img">元の画像</param>
        static public void ImageMultiply(Vec4b color, ref Mat<Vec4b> img)
        {
            var indexer = img.GetIndexer();
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    indexer[y, x] = new Vec4b(
                        (byte)(color.Item0 * (indexer[y, x].Item0 / 255.0F)),
                        (byte)(color.Item1 * (indexer[y, x].Item1 / 255.0F)),
                        (byte)(color.Item2 * (indexer[y, x].Item2 / 255.0F)),
                        indexer[y, x].Item3
                    );
                }
            }

            return;
        }

        /// <summary>
        /// 指定された画像からマスク画像を生成する。
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        static public Mat<Vec4b> GenerateMaskImage(in Mat<Vec4b> img)
        {
            Mat<Vec4b> mask = new Mat<Vec4b>(img.Height, img.Width);
            var indexer = img.GetIndexer();
            var maskIndexer = mask.GetIndexer();
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    byte alpha = indexer[y, x].Item3;

                    // 元の画像のアルファ値をマスクのRGB値に設定
                    maskIndexer[y, x] = new Vec4b(alpha, alpha, alpha, 255);
                }
            }

            return mask;
        }

        /// <summary>
        /// 指定された色で画像を乗算する。
        /// </summary>
        /// <param name="color">乗算する色</param>
        /// <param name="img">元の画像</param>
        static public void ImageMultiply(string color, ref Mat<Vec4b> img)
        {
            // colorは"#"なしのWebカラー（例: "FFAABB" または "FFAABBCC"）
            byte r = 0, g = 0, b = 0, a = 255;
            if ((color.Length == 6) || (color.Length == 8))
            {
                r = Convert.ToByte(color.Substring(0, 2), 16);
                g = Convert.ToByte(color.Substring(2, 2), 16);
                b = Convert.ToByte(color.Substring(4, 2), 16);
                // アルファ値は無視して255固定
            }
            else
            {
                // 無効なカラーコードの場合は何もしない
                return;
            }

            Vec4b vecColor = new Vec4b(b, g, r, a);
            ImageMultiply(vecColor, ref img);
            return;
        }
    }
}
