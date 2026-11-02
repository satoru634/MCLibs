using MCToolsCommonLib.BaseData;

namespace MCLibsTests.MCToolsCommonLib.BaseData
{
    /// <summary>
    /// PointXZ のテスト
    /// </summary>
    public class PointXZTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Default_InitializesToZero()
        {
            var point = new PointXZ();

            Assert.Equal(0, point.X);
            Assert.Equal(0, point.Z);
        }

        [Fact]
        public void Constructor_WithValues_SetsXZ()
        {
            var point = new PointXZ(7, -8);

            Assert.Equal(7, point.X);
            Assert.Equal(-8, point.Z);
        }

        [Fact]
        public void Constructor_Copy_CopiesValuesAsIndependentInstance()
        {
            var source = new PointXZ(1, 2);

            var copy = new PointXZ(source);
            source.X = 100;

            Assert.Equal(1, copy.X);
            Assert.Equal(2, copy.Z);
        }

        // ── Max / Min ─────────────────────────────────────────────────────────

        [Fact]
        public void Max_UpdatesEachAxisToLargerValue()
        {
            var point = new PointXZ(5, 5);

            point.Max(10, 1);

            Assert.Equal(10, point.X);
            Assert.Equal(5, point.Z);
        }

        [Fact]
        public void Min_UpdatesEachAxisToSmallerValue()
        {
            var point = new PointXZ(5, 5);

            point.Min(10, 1);

            Assert.Equal(5, point.X);
            Assert.Equal(1, point.Z);
        }

        // ── Shift / ShiftAll ──────────────────────────────────────────────────

        [Fact]
        public void Shift_AddsAmountToEachAxis()
        {
            var point = new PointXZ(1, 2);

            point.Shift(3, -4);

            Assert.Equal(4, point.X);
            Assert.Equal(-2, point.Z);
        }

        [Fact]
        public void ShiftAll_AddsSameAmountToAllAxes()
        {
            var point = new PointXZ(1, 2);

            point.ShiftAll(10);

            Assert.Equal(11, point.X);
            Assert.Equal(12, point.Z);
        }

        // ── ToString ──────────────────────────────────────────────────────────

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            var point = new PointXZ(1, 2);

            Assert.Equal("X: 1, Z:2", point.ToString());
        }
    }
}
