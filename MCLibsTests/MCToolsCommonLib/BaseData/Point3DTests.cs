using MCToolsCommonLib.BaseData;

namespace MCLibsTests.MCToolsCommonLib.BaseData
{
    /// <summary>
    /// Point3D のテスト
    /// </summary>
    public class Point3DTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Default_InitializesToZero()
        {
            var point = new Point3D();

            Assert.Equal(0, point.X);
            Assert.Equal(0, point.Y);
            Assert.Equal(0, point.Z);
        }

        [Fact]
        public void Constructor_WithValues_SetsXYZ()
        {
            var point = new Point3D(1, -2, 3);

            Assert.Equal(1, point.X);
            Assert.Equal(-2, point.Y);
            Assert.Equal(3, point.Z);
        }

        [Fact]
        public void Constructor_Copy_CopiesValuesAsIndependentInstance()
        {
            var source = new Point3D(4, 5, 6);

            var copy = new Point3D(source);
            source.X = 100;

            Assert.Equal(4, copy.X);
            Assert.Equal(5, copy.Y);
            Assert.Equal(6, copy.Z);
        }

        // ── Max / Min ─────────────────────────────────────────────────────────

        [Fact]
        public void Max_UpdatesEachAxisToLargerValue()
        {
            var point = new Point3D(5, 5, 5);

            point.Max(10, 1, 5);

            Assert.Equal(10, point.X);
            Assert.Equal(5, point.Y);
            Assert.Equal(5, point.Z);
        }

        [Fact]
        public void Min_UpdatesEachAxisToSmallerValue()
        {
            var point = new Point3D(5, 5, 5);

            point.Min(10, 1, -3);

            Assert.Equal(5, point.X);
            Assert.Equal(1, point.Y);
            Assert.Equal(-3, point.Z);
        }

        // ── Shift / ShiftAll ──────────────────────────────────────────────────

        [Fact]
        public void Shift_AddsAmountToEachAxis()
        {
            var point = new Point3D(1, 2, 3);

            point.Shift(10, -20, 0);

            Assert.Equal(11, point.X);
            Assert.Equal(-18, point.Y);
            Assert.Equal(3, point.Z);
        }

        [Fact]
        public void ShiftAll_AddsSameAmountToAllAxes()
        {
            var point = new Point3D(1, 2, 3);

            point.ShiftAll(-5);

            Assert.Equal(-4, point.X);
            Assert.Equal(-3, point.Y);
            Assert.Equal(-2, point.Z);
        }

        // ── ToString ──────────────────────────────────────────────────────────

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            var point = new Point3D(1, 2, 3);

            Assert.Equal("X: 1, Y: 2, Z:3", point.ToString());
        }
    }
}
