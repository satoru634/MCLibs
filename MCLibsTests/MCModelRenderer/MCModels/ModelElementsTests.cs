using System.Windows.Media.Media3D;
using MCLibsTests.Helpers;
using MCModelRenderer.MCModels;

namespace MCLibsTests.MCModelRenderer.MCModels
{
    /// <summary>
    /// ModelElements のテスト
    /// </summary>
    public class ModelElementsTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Default_InitializesEmptyElement()
        {
            using var element = new ModelElements();

            Assert.Equal(new Point3D(0, 0, 0), element.From);
            Assert.Equal(new Point3D(0, 0, 0), element.To);
            Assert.Equal("", element.Rotate.Axis);
            Assert.Empty(element.Faces);
            Assert.Equal(-8.0f, element.MoveAmount);
        }

        [Fact]
        public void Constructor_WithValues_SetsAllProperties()
        {
            var rotation = new ModelRotation();
            var faces = new Dictionary<string, ModelFace> { ["up"] = new ModelFace() };

            using var element = new ModelElements(new Point3D(1, 2, 3), new Point3D(4, 5, 6), rotation, faces, -4.0f);

            Assert.Equal(new Point3D(1, 2, 3), element.From);
            Assert.Equal(new Point3D(4, 5, 6), element.To);
            Assert.Same(rotation, element.Rotate);
            Assert.Same(faces, element.Faces);
            Assert.Equal(-4.0f, element.MoveAmount);
        }

        // ── AddPos ────────────────────────────────────────────────────────────

        [Fact]
        public void AddPos_From_SetsFrom()
        {
            using var element = new ModelElements();

            element.AddPos(TestJson.ToObject("[0, 0.5, 1]"), "from");

            Assert.Equal(new Point3D(0, 0.5, 1), element.From);
            Assert.Equal(new Point3D(0, 0, 0), element.To);
        }

        [Fact]
        public void AddPos_To_SetsTo()
        {
            using var element = new ModelElements();

            element.AddPos(TestJson.ToObject("[16, 8, 16]"), "to");

            Assert.Equal(new Point3D(16, 8, 16), element.To);
        }

        [Fact]
        public void AddPos_NotThreeValues_IsIgnored()
        {
            using var element = new ModelElements();

            element.AddPos(TestJson.ToObject("[1, 2]"), "from");

            Assert.Equal(new Point3D(0, 0, 0), element.From);
        }

        [Fact]
        public void AddPos_UnknownKey_IsIgnored()
        {
            using var element = new ModelElements();

            element.AddPos(TestJson.ToObject("[1, 2, 3]"), "center");

            Assert.Equal(new Point3D(0, 0, 0), element.From);
            Assert.Equal(new Point3D(0, 0, 0), element.To);
        }

        // ── AddRotation ───────────────────────────────────────────────────────

        [Fact]
        public void AddRotation_SetsRotate()
        {
            using var element = new ModelElements();

            element.AddRotation(TestJson.ToObject("""{ "origin": [8, 8, 8], "axis": "x", "angle": 45 }"""));

            Assert.Equal("x", element.Rotate.Axis);
            Assert.Equal(45.0, element.Rotate.Angle);
            Assert.Equal(new Point3D(8, 8, 8), element.Rotate.Origin);
        }

        // ── AddFaces ──────────────────────────────────────────────────────────

        [Fact]
        public void AddFaces_AddsEachFace()
        {
            using var element = new ModelElements();

            element.AddFaces(TestJson.ToObject("""{ "up": { "texture": "#top" }, "down": { "texture": "#bottom" } }"""));

            Assert.Equal(2, element.Faces.Count);
            Assert.Equal("#top", element.Faces["up"].Texture);
            Assert.Equal("#bottom", element.Faces["down"].Texture);
        }

        [Fact]
        public void AddFaces_DuplicateFace_ThrowsArgumentException()
        {
            using var element = new ModelElements();
            element.AddFaces(TestJson.ToObject("""{ "up": { "texture": "#top" } }"""));

            Assert.Throws<ArgumentException>(() => element.AddFaces(TestJson.ToObject("""{ "up": { "texture": "#other" } }""")));
        }

        // ── Dispose ───────────────────────────────────────────────────────────

        [Fact]
        public void Dispose_ClearsFaces()
        {
            var element = new ModelElements();
            element.AddFaces(TestJson.ToObject("""{ "up": { "texture": "#top" } }"""));

            element.Dispose();

            Assert.Empty(element.Faces);
        }
    }
}
