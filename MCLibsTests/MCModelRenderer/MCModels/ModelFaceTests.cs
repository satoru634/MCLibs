using MCLibsTests.Helpers;
using MCModelRenderer.MCModels;
using OpenCvSharp;

namespace MCLibsTests.MCModelRenderer.MCModels
{
    /// <summary>
    /// ModelFace のテスト
    /// </summary>
    public class ModelFaceTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Default_UsesFullUV()
        {
            using var face = new ModelFace();

            Assert.Equal(new Rect(0, 0, 16, 16), face.UV);
            Assert.Equal(0, face.Rotate);
            Assert.Equal("", face.Texture);
            Assert.Empty(face.TextureFlip);
            Assert.Equal("", face.Cullface);
        }

        [Fact]
        public void Constructor_WithValues_SetsAllProperties()
        {
            var flips = new List<FlipMode> { FlipMode.X };

            using var face = new ModelFace(new Rect(1, 2, 3, 4), 90, "#side", flips, "north");

            Assert.Equal(new Rect(1, 2, 3, 4), face.UV);
            Assert.Equal(90, face.Rotate);
            Assert.Equal("#side", face.Texture);
            Assert.Same(flips, face.TextureFlip);
            Assert.Equal("north", face.Cullface);
        }

        [Fact]
        public void Constructor_FromJson_SetsAllProperties()
        {
            using var face = new ModelFace(TestJson.ToObject(
                """{ "uv": [0, 4, 16, 12], "texture": "#top", "rotation": 180, "cullface": "up", "tintindex": 0 }"""));

            Assert.Equal(new Rect(0, 4, 16, 8), face.UV);
            Assert.Equal("#top", face.Texture);
            Assert.Equal(180, face.Rotate);
            Assert.Equal("up", face.Cullface);
            Assert.Empty(face.TextureFlip);
        }

        [Fact]
        public void Constructor_FromJson_WithoutUV_UsesFullUV()
        {
            using var face = new ModelFace(TestJson.ToObject("""{ "texture": "#all" }"""));

            Assert.Equal(new Rect(0, 0, 16, 16), face.UV);
        }

        [Fact]
        public void Constructor_FromJson_ReversedU_AddsFlipY()
        {
            using var face = new ModelFace(TestJson.ToObject("""{ "uv": [16, 0, 0, 16] }"""));

            Assert.Equal(new Rect(0, 0, 16, 16), face.UV);
            Assert.Equal([FlipMode.Y], face.TextureFlip);
        }

        [Fact]
        public void Constructor_FromJson_ReversedV_AddsFlipX()
        {
            using var face = new ModelFace(TestJson.ToObject("""{ "uv": [2, 14, 6, 10] }"""));

            Assert.Equal(new Rect(2, 10, 4, 4), face.UV);
            Assert.Equal([FlipMode.X], face.TextureFlip);
        }

        [Fact]
        public void Constructor_FromJson_ReversedUV_AddsBothFlips()
        {
            using var face = new ModelFace(TestJson.ToObject("""{ "uv": [16, 16, 0, 0] }"""));

            Assert.Equal([FlipMode.Y, FlipMode.X], face.TextureFlip);
        }

        [Fact]
        public void Constructor_FromJson_UVNotFourValues_UsesFullUV()
        {
            using var face = new ModelFace(TestJson.ToObject("""{ "uv": [0, 0, 8] }"""));

            Assert.Equal(new Rect(0, 0, 16, 16), face.UV);
        }

        // ── Dispose ───────────────────────────────────────────────────────────

        [Fact]
        public void Dispose_ClearsTextureFlip()
        {
            var face = new ModelFace(TestJson.ToObject("""{ "uv": [16, 16, 0, 0] }"""));

            face.Dispose();

            Assert.Empty(face.TextureFlip);
        }
    }
}
