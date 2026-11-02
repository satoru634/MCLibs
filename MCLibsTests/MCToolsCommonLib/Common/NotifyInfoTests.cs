using MCToolsCommonLib.Common;

namespace MCLibsTests.MCToolsCommonLib.Common
{
    /// <summary>
    /// NotifyInfo のテスト
    /// </summary>
    public class NotifyInfoTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Default_MessageIsEmptyAndColorIsWhite()
        {
            var info = new NotifyInfo();

            Assert.Equal("", info.Message);
            Assert.Equal("#FFFFFF", info.Color);
        }

        [Fact]
        public void Constructor_WithValues_SetsMessageAndColor()
        {
            var info = new NotifyInfo("出力しました", "#FF0000");

            Assert.Equal("出力しました", info.Message);
            Assert.Equal("#FF0000", info.Color);
        }

        [Fact]
        public void Constructor_Copy_CopiesValuesAsIndependentInstance()
        {
            var source = new NotifyInfo("message", "#00FF00");

            var copy = new NotifyInfo(source);
            source.Message = "changed";

            Assert.Equal("message", copy.Message);
            Assert.Equal("#00FF00", copy.Color);
        }
    }
}
