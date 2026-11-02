using System.Windows.Media.Media3D;
using MCLibsTests.Helpers;
using MCModelRenderer.MCModels;

namespace MCLibsTests.MCModelRenderer.MCModels
{
    /// <summary>
    /// ModelDisplay のテスト
    /// </summary>
    public class ModelDisplayTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Default_InitializesToZero()
        {
            var display = new ModelDisplay();

            Assert.Equal(new Point3D(0, 0, 0), display.Rotation);
            Assert.Equal(new Point3D(0, 0, 0), display.Translation);
            Assert.Equal(new Point3D(0, 0, 0), display.Scale);
        }

        [Fact]
        public void Constructor_FromJson_SetsAllValues()
        {
            var display = new ModelDisplay(TestJson.ToObject(
                """{ "rotation": [30, 225, 0], "translation": [0, 1.5, -2], "scale": [0.625, 0.625, 0.625] }"""));

            Assert.Equal(new Point3D(30, 225, 0), display.Rotation);
            Assert.Equal(new Point3D(0, 1.5, -2), display.Translation);
            Assert.Equal(new Point3D(0.625, 0.625, 0.625), display.Scale);
        }

        [Fact]
        public void Constructor_FromJson_MissingKeys_RemainZero()
        {
            var display = new ModelDisplay(TestJson.ToObject("""{ "rotation": [0, 90, 0] }"""));

            Assert.Equal(new Point3D(0, 90, 0), display.Rotation);
            Assert.Equal(new Point3D(0, 0, 0), display.Translation);
            Assert.Equal(new Point3D(0, 0, 0), display.Scale);
        }
    }
}
