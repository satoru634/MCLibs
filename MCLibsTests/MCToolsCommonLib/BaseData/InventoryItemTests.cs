using MCToolsCommonLib.BaseData;

namespace MCLibsTests.MCToolsCommonLib.BaseData
{
    /// <summary>
    /// InventoryItem のテスト
    /// </summary>
    public class InventoryItemTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Default_InitializesToEmpty()
        {
            var item = new InventoryItem();

            Assert.Equal("", item.Name);
            Assert.Equal(0, item.Count);
        }

        [Fact]
        public void Constructor_WithValues_SetsNameAndCount()
        {
            var item = new InventoryItem("minecraft:stone", 64);

            Assert.Equal("minecraft:stone", item.Name);
            Assert.Equal(64, item.Count);
        }
    }
}
