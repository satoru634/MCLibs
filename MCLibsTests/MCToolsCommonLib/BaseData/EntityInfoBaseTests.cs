using MCToolsCommonLib.BaseData;

namespace MCLibsTests.MCToolsCommonLib.BaseData
{
    /// <summary>
    /// EntityInfoBase のテスト
    /// </summary>
    public class EntityInfoBaseTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Default_InitializesStringsToEmpty()
        {
            var entity = new EntityInfoBase();

            Assert.Equal("", entity.Name);
            Assert.Equal("", entity.UUID);
        }

        [Fact]
        public void Constructor_Default_InitializesPositionsToOrigin()
        {
            var entity = new EntityInfoBase();

            Assert.Equal("X: 0, Y: 0, Z:0", entity.CoordinatePos.ToString());
            Assert.Equal("X: 0, Z:0", entity.RegionPos.ToString());
            Assert.Equal("X: 0, Z:0", entity.ChunkPos.ToString());
            Assert.Equal("X: 0, Z:0", entity.WorldChunkPos.ToString());
        }

        [Fact]
        public void Constructor_Default_PositionsAreIndependentInstances()
        {
            var entity = new EntityInfoBase();

            Assert.NotSame(entity.RegionPos, entity.ChunkPos);
            Assert.NotSame(entity.ChunkPos, entity.WorldChunkPos);
        }

        [Fact]
        public void Constructor_Default_ItemInfoIsEmpty()
        {
            var entity = new EntityInfoBase();

            Assert.Empty(entity.ItemInfo);
        }
    }
}
