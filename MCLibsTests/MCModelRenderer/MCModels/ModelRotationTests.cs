using System.Text.Json;
using System.Windows.Media.Media3D;
using MCModelRenderer.MCModels;

namespace MCLibsTests.MCModelRenderer.MCModels
{
    /// <summary>
    /// ModelRotation のテスト
    /// </summary>
    public class ModelRotationTests
    {
        /// <summary>
        /// JSON 文字列から、プロダクトコードと同じ形の回転データを作成する。
        /// </summary>
        private static Dictionary<string, object> Parse(string json)
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;
        }

        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Default_InitializesToNoRotation()
        {
            var rotation = new ModelRotation();

            Assert.Equal(0.0, rotation.Angle);
            Assert.Equal("", rotation.Axis);
            Assert.Equal(new Point3D(0, 0, 0), rotation.Origin);
        }

        [Fact]
        public void Constructor_FromJson_SetsAllValues()
        {
            var rotation = new ModelRotation(Parse("""{ "origin": [8, 8, 8.5], "axis": "y", "angle": -22.5 }"""));

            Assert.Equal(-22.5, rotation.Angle);
            Assert.Equal("y", rotation.Axis);
            Assert.Equal(new Point3D(8, 8, 8.5), rotation.Origin);
        }

        [Fact]
        public void Constructor_FromJson_OriginNotThreeValues_UsesZero()
        {
            var rotation = new ModelRotation(Parse("""{ "origin": [8, 8], "axis": "x", "angle": 45 }"""));

            Assert.Equal(new Point3D(0, 0, 0), rotation.Origin);
            Assert.Equal(45.0, rotation.Angle);
        }

        [Fact]
        public void Constructor_FromJson_UnknownKeysAreIgnored()
        {
            var rotation = new ModelRotation(Parse("""{ "rescale": true, "axis": "z" }"""));

            Assert.Equal("z", rotation.Axis);
            Assert.Equal(0.0, rotation.Angle);
        }
    }
}
