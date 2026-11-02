using MCLibsTests.Helpers;
using MCToolsCommonLib.Utils;

namespace MCLibsTests.MCToolsCommonLib.Utils
{
    /// <summary>
    /// ConvertJsonValue のテスト
    /// </summary>
    public class ConvertJsonValueTests
    {
        // ── ConvertInt ────────────────────────────────────────────────────────

        [Fact]
        public void ConvertInt_JsonNumber_ReturnsInt()
        {
            Assert.Equal(90, ConvertJsonValue.ConvertInt(TestJson.ToObject("90")));
        }

        [Fact]
        public void ConvertInt_NumericString_ReturnsInt()
        {
            Assert.Equal(-12, ConvertJsonValue.ConvertInt("-12"));
        }

        [Fact]
        public void ConvertInt_NonNumeric_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => ConvertJsonValue.ConvertInt("abc"));
        }

        // ── ConvertDouble ─────────────────────────────────────────────────────

        [Fact]
        public void ConvertDouble_JsonNumber_ReturnsDouble()
        {
            Assert.Equal(22.5, ConvertJsonValue.ConvertDouble(TestJson.ToObject("22.5")));
        }

        [Fact]
        public void ConvertDouble_IntegerJsonNumber_ReturnsDouble()
        {
            Assert.Equal(-45.0, ConvertJsonValue.ConvertDouble(TestJson.ToObject("-45")));
        }

        // ── ConvertStr ────────────────────────────────────────────────────────

        [Fact]
        public void ConvertStr_JsonString_ReturnsStringWithoutQuotes()
        {
            Assert.Equal("#texture", ConvertJsonValue.ConvertStr(TestJson.ToObject("\"#texture\"")));
        }

        [Fact]
        public void ConvertStr_JsonNumber_ReturnsNumberText()
        {
            Assert.Equal("16", ConvertJsonValue.ConvertStr(TestJson.ToObject("16")));
        }
    }
}
