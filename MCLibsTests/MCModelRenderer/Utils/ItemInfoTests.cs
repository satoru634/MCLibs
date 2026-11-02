using MCModelRenderer.Utils;

namespace MCLibsTests.MCModelRenderer.Utils
{
    /// <summary>
    /// ItemInfo のテスト
    /// </summary>
    public class ItemInfoTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Default_InitializesToEmpty()
        {
            var info = new ItemInfo();

            Assert.Equal("", info.Type);
            Assert.Equal("", info.Name);
        }

        [Fact]
        public void Constructor_WithValues_SetsTypeAndName()
        {
            var info = new ItemInfo("block", "Stone");

            Assert.Equal("block", info.Type);
            Assert.Equal("Stone", info.Name);
        }
    }
}
