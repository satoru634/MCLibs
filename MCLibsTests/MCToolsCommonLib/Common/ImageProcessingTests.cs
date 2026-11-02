using MCToolsCommonLib.Common;
using OpenCvSharp;

namespace MCLibsTests.MCToolsCommonLib.Common
{
    /// <summary>
    /// ImageProcessing のテスト
    /// </summary>
    public class ImageProcessingTests
    {
        /// <summary>
        /// 単色の画像を作成する。
        /// </summary>
        private static Mat<Vec4b> CreateImage(int width, int height, Vec4b color)
        {
            var img = new Mat<Vec4b>(height, width);
            img.SetTo(new Scalar(color.Item0, color.Item1, color.Item2, color.Item3));
            return img;
        }

        // ── ImageOverlay ──────────────────────────────────────────────────────

        [Fact]
        public void ImageOverlay_OpaquePixels_OverwriteBase()
        {
            using var overlay = CreateImage(2, 2, new Vec4b(10, 20, 30, 128));
            var baseImg = CreateImage(2, 2, new Vec4b(0, 0, 0, 255));

            ImageProcessing.ImageOverlay(overlay, ref baseImg);

            Assert.Equal(new Vec4b(10, 20, 30, 128), baseImg.GetIndexer()[1, 1]);
            baseImg.Dispose();
        }

        [Fact]
        public void ImageOverlay_TransparentPixels_KeepBase()
        {
            using var overlay = CreateImage(2, 2, new Vec4b(10, 20, 30, 0));
            var baseImg = CreateImage(2, 2, new Vec4b(1, 2, 3, 255));

            ImageProcessing.ImageOverlay(overlay, ref baseImg);

            Assert.Equal(new Vec4b(1, 2, 3, 255), baseImg.GetIndexer()[0, 0]);
            baseImg.Dispose();
        }

        [Fact]
        public void ImageOverlay_WithDrawStart_DrawsAtOffset()
        {
            using var overlay = CreateImage(1, 1, new Vec4b(255, 255, 255, 255));
            var baseImg = CreateImage(3, 3, new Vec4b(0, 0, 0, 255));

            ImageProcessing.ImageOverlay(overlay, ref baseImg, new Point(2, 1));

            var indexer = baseImg.GetIndexer();
            Assert.Equal(new Vec4b(255, 255, 255, 255), indexer[1, 2]);   // [y, x]
            Assert.Equal(new Vec4b(0, 0, 0, 255), indexer[0, 0]);
            baseImg.Dispose();
        }

        // ── ImageMultiply(Vec4b) ──────────────────────────────────────────────

        [Fact]
        public void ImageMultiply_Vec4b_MultipliesBGRAndKeepsAlpha()
        {
            var img = CreateImage(1, 1, new Vec4b(255, 128, 0, 100));

            ImageProcessing.ImageMultiply(new Vec4b(200, 200, 200, 0), ref img);

            // 200 * (255/255) = 200、200 * (128/255) ≒ 100.39 → 100、200 * 0 = 0
            Assert.Equal(new Vec4b(200, 100, 0, 100), img.GetIndexer()[0, 0]);
            img.Dispose();
        }

        // ── ImageMultiply(string) ─────────────────────────────────────────────

        [Fact]
        public void ImageMultiply_String_RRGGBB_IsAppliedInBGROrder()
        {
            var img = CreateImage(1, 1, new Vec4b(255, 255, 255, 255));

            ImageProcessing.ImageMultiply("FF8000", ref img);

            Assert.Equal(new Vec4b(0x00, 0x80, 0xFF, 255), img.GetIndexer()[0, 0]);
            img.Dispose();
        }

        [Fact]
        public void ImageMultiply_String_EightDigits_IgnoresAlphaPart()
        {
            var img = CreateImage(1, 1, new Vec4b(255, 255, 255, 50));

            ImageProcessing.ImageMultiply("102030FF", ref img);

            Assert.Equal(new Vec4b(0x30, 0x20, 0x10, 50), img.GetIndexer()[0, 0]);
            img.Dispose();
        }

        [Theory]
        [InlineData("")]
        [InlineData("FFF")]
        [InlineData("#FFFFFF")]
        public void ImageMultiply_String_InvalidLength_DoesNothing(string color)
        {
            var img = CreateImage(1, 1, new Vec4b(10, 20, 30, 40));

            ImageProcessing.ImageMultiply(color, ref img);

            Assert.Equal(new Vec4b(10, 20, 30, 40), img.GetIndexer()[0, 0]);
            img.Dispose();
        }

        // ── GenerateMaskImage ─────────────────────────────────────────────────

        [Fact]
        public void GenerateMaskImage_UsesAlphaAsGrayscale()
        {
            using var img = CreateImage(2, 1, new Vec4b(10, 20, 30, 0));
            img.GetIndexer()[0, 1] = new Vec4b(10, 20, 30, 200);

            using var mask = ImageProcessing.GenerateMaskImage(img);

            Assert.Equal(img.Width, mask.Width);
            Assert.Equal(img.Height, mask.Height);
            Assert.Equal(new Vec4b(0, 0, 0, 255), mask.GetIndexer()[0, 0]);
            Assert.Equal(new Vec4b(200, 200, 200, 255), mask.GetIndexer()[0, 1]);
        }
    }
}
