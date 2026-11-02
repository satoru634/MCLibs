using MCModelRenderer.Utils;
using OpenCvSharp;
using SharpDX;
using WpfColor = System.Windows.Media.Color;

namespace MCLibsTests.MCModelRenderer.Utils
{
    /// <summary>
    /// ColorConverter のテスト
    /// </summary>
    public class ColorConverterTests
    {
        // ── StringToColor4 ────────────────────────────────────────────────────

        [Fact]
        public void StringToColor4_RRGGBB_ReturnsOpaqueColor()
        {
            var color = ColorConverter.StringToColor4("#FF8000");

            Assert.Equal(new Color4(1.0f, 128 / 255f, 0.0f, 1.0f), color);
        }

        [Fact]
        public void StringToColor4_AARRGGBB_ReturnsColorWithAlpha()
        {
            var color = ColorConverter.StringToColor4("#80FF0000");

            Assert.Equal(new Color4(1.0f, 0.0f, 0.0f, 128 / 255f), color);
        }

        [Theory]
        [InlineData("")]
        [InlineData("#FFF")]
        public void StringToColor4_InvalidLength_ReturnsTransparentBlack(string input)
        {
            Assert.Equal(new Color4(0, 0, 0, 0), ColorConverter.StringToColor4(input));
        }

        // ── StringToWpfColor ──────────────────────────────────────────────────

        [Fact]
        public void StringToWpfColor_RRGGBB_ReturnsOpaqueColor()
        {
            Assert.Equal(WpfColor.FromArgb(255, 0x12, 0x34, 0x56), ColorConverter.StringToWpfColor("#123456"));
        }

        [Fact]
        public void StringToWpfColor_AARRGGBB_ReturnsColorWithAlpha()
        {
            Assert.Equal(WpfColor.FromArgb(0x00, 0x12, 0x34, 0x56), ColorConverter.StringToWpfColor("#00123456"));
        }

        [Fact]
        public void StringToWpfColor_InvalidLength_ReturnsOpaqueBlack()
        {
            Assert.Equal(WpfColor.FromArgb(255, 0, 0, 0), ColorConverter.StringToWpfColor("red"));
        }

        [Fact]
        public void StringToWpfColor_InvalidHex_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => ColorConverter.StringToWpfColor("#GGGGGG"));
        }

        // ── StringToVec4b ─────────────────────────────────────────────────────

        [Fact]
        public void StringToVec4b_RRGGBB_ReturnsBGRA()
        {
            Assert.Equal(new Vec4b(0x56, 0x34, 0x12, 255), ColorConverter.StringToVec4b("#123456"));
        }

        [Fact]
        public void StringToVec4b_AARRGGBB_ReturnsBGRA()
        {
            Assert.Equal(new Vec4b(0x56, 0x34, 0x12, 0x80), ColorConverter.StringToVec4b("#80123456"));
        }

        [Fact]
        public void StringToVec4b_InvalidLength_ReturnsOpaqueBlack()
        {
            Assert.Equal(new Vec4b(0, 0, 0, 255), ColorConverter.StringToVec4b("123456"));
        }

        // ── Color4ToVec4b ─────────────────────────────────────────────────────

        [Fact]
        public void Color4ToVec4b_ReturnsBGRA()
        {
            var vec = ColorConverter.Color4ToVec4b(new Color4(1.0f, 0.5f, 0.0f, 1.0f));

            // 0.5 * 255 = 127.5 → 127（切り捨て）
            Assert.Equal(new Vec4b(0, 127, 255, 255), vec);
        }

        [Fact]
        public void Color4ToVec4b_RoundTripWithStringToColor4()
        {
            var vec = ColorConverter.Color4ToVec4b(ColorConverter.StringToColor4("#80FF0000"));

            Assert.Equal(new Vec4b(0, 0, 255, 128), vec);
        }

        // ── CheckColorFormat ──────────────────────────────────────────────────

        [Theory]
        [InlineData("#000000", true)]
        [InlineData("#abcdef", true)]
        [InlineData("#FF00FF00", true)]
        [InlineData("000000", false)]       // # なし
        [InlineData("#00000", false)]       // 桁数不足
        [InlineData("#0000000", false)]     // 8桁（7・9文字以外）
        [InlineData("#GG0000", false)]      // 16進数以外
        [InlineData("", false)]
        public void CheckColorFormat_ReturnsExpectedResult(string color, bool expected)
        {
            Assert.Equal(expected, ColorConverter.CheckColorFormat(color));
        }
    }
}
