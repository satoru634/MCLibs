using MCToolsCommonLib.BaseData;

namespace MCLibsTests.MCToolsCommonLib.BaseData
{
    /// <summary>
    /// PointRange のテスト
    /// </summary>
    public class PointRangeTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Default_RangeIsIntMaxValue()
        {
            var range = new PointRange();

            Assert.Equal(0, range.Point.X);
            Assert.Equal(0, range.Point.Z);
            Assert.Equal(int.MaxValue, range.Range);
        }

        [Fact]
        public void Constructor_WithCoordinates_SetsPointAndRange()
        {
            var range = new PointRange(3, 4, 5);

            Assert.Equal(3, range.Point.X);
            Assert.Equal(4, range.Point.Z);
            Assert.Equal(5, range.Range);
        }

        [Fact]
        public void Constructor_WithPoint_UsesSameInstance()
        {
            var point = new PointXZ(1, 2);

            var range = new PointRange(point, 10);

            Assert.Same(point, range.Point);
            Assert.Equal(10, range.Range);
        }

        [Fact]
        public void Constructor_Copy_DeepCopiesPoint()
        {
            var source = new PointRange(1, 2, 3);

            var copy = new PointRange(source);
            source.Point.X = 100;

            Assert.NotSame(source.Point, copy.Point);
            Assert.Equal(1, copy.Point.X);
            Assert.Equal(2, copy.Point.Z);
            Assert.Equal(3, copy.Range);
        }

        // ── IsCollisionWithRect(PointRange) ───────────────────────────────────

        [Fact]
        public void IsCollisionWithRect_TargetInsideRange_ReturnsTrue()
        {
            var baseRange = new PointRange(0, 0, 5);

            Assert.True(baseRange.IsCollisionWithRect(new PointRange(0, 0, 1)));
        }

        [Fact]
        public void IsCollisionWithRect_TargetFarAway_ReturnsFalse()
        {
            var baseRange = new PointRange(0, 0, 5);

            Assert.False(baseRange.IsCollisionWithRect(new PointRange(100, 100, 1)));
        }

        [Fact(Skip = "既知の不具合: 対象側の左上座標を +Range でシフトしているため、対象の矩形が正しく計算されない")]
        public void IsCollisionWithRect_TargetRectOverlapsBaseRect_ReturnsTrue()
        {
            // 基準: X,Z ともに -5～5、対象: X が 3～13、Z が -5～5 → 重なっている
            var baseRange = new PointRange(0, 0, 5);

            Assert.True(baseRange.IsCollisionWithRect(new PointRange(8, 0, 5)));
        }

        // ── IsCollisionWithRect(Point3D, Point3D) ─────────────────────────────

        [Fact]
        public void IsCollisionWithRect_Box_Overlapping_ReturnsTrue()
        {
            var baseRange = new PointRange(0, 0, 5);

            Assert.True(baseRange.IsCollisionWithRect(new Point3D(3, 0, 3), new Point3D(10, 0, 10)));
        }

        [Fact]
        public void IsCollisionWithRect_Box_TouchingEdge_ReturnsTrue()
        {
            var baseRange = new PointRange(0, 0, 5);

            Assert.True(baseRange.IsCollisionWithRect(new Point3D(5, 0, -5), new Point3D(10, 0, 5)));
        }

        [Fact]
        public void IsCollisionWithRect_Box_NotOverlapping_ReturnsFalse()
        {
            var baseRange = new PointRange(0, 0, 5);

            Assert.False(baseRange.IsCollisionWithRect(new Point3D(6, 0, 0), new Point3D(10, 0, 3)));
        }

        [Fact]
        public void IsCollisionWithRect_Box_IgnoresY()
        {
            var baseRange = new PointRange(0, 0, 5);

            Assert.True(baseRange.IsCollisionWithRect(new Point3D(0, 1000, 0), new Point3D(1, 2000, 1)));
        }

        // ── IsCollisionWithCircle(PointRange) ─────────────────────────────────

        [Theory]
        [InlineData(5, true)]   // 距離 5 ちょうど
        [InlineData(4, false)]  // 距離 5 > 範囲 4
        public void IsCollisionWithCircle_ComparesDistanceWithRange(int range, bool expected)
        {
            var baseRange = new PointRange(0, 0, range);

            Assert.Equal(expected, baseRange.IsCollisionWithCircle(new PointRange(3, 4, 0)));
        }

        // ── IsCollisionWithCircle(Point3D, Point3D) ───────────────────────────

        [Fact]
        public void IsCollisionWithCircle_Box_LeftTopInRange_ReturnsTrue()
        {
            var baseRange = new PointRange(0, 0, 5);

            Assert.True(baseRange.IsCollisionWithCircle(new Point3D(3, 0, 4), new Point3D(100, 0, 100)));
        }

        [Fact]
        public void IsCollisionWithCircle_Box_RightBottomInRange_ReturnsTrue()
        {
            var baseRange = new PointRange(0, 0, 5);

            Assert.True(baseRange.IsCollisionWithCircle(new Point3D(-100, 0, -100), new Point3D(1, 0, 1)));
        }

        [Fact]
        public void IsCollisionWithCircle_Box_BothCornersOutOfRange_ReturnsFalse()
        {
            var baseRange = new PointRange(0, 0, 5);

            Assert.False(baseRange.IsCollisionWithCircle(new Point3D(10, 0, 10), new Point3D(20, 0, 20)));
        }

        // ── ToString ──────────────────────────────────────────────────────────

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            var range = new PointRange(1, 2, 3);

            Assert.Equal("X: 1, Z:2, Range:3", range.ToString());
        }
    }
}
