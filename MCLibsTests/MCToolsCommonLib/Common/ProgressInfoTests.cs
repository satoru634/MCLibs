using MCToolsCommonLib.Common;

namespace MCLibsTests.MCToolsCommonLib.Common
{
    /// <summary>
    /// ProgressInfo のテスト
    /// </summary>
    public class ProgressInfoTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Default_InitializesToZeroAndEmpty()
        {
            var info = new ProgressInfo();

            Assert.Equal(0, info.ProgressValue);
            Assert.Equal("", info.ProgressText);
        }

        [Fact]
        public void Constructor_WithValues_SetsValueAndText()
        {
            var info = new ProgressInfo(50, "処理中");

            Assert.Equal(50, info.ProgressValue);
            Assert.Equal("処理中", info.ProgressText);
        }
    }
}
