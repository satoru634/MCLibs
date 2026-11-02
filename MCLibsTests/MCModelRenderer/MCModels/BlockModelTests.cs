using System.Text.Json;
using System.Windows.Media.Media3D;
using MCModelRenderer.MCModels;
using OpenCvSharp;

namespace MCLibsTests.MCModelRenderer.MCModels
{
    /// <summary>
    /// BlockModel のテスト
    /// </summary>
    public class BlockModelTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Default_InitializesEmptyModel()
        {
            using var model = new BlockModel();

            Assert.Equal("", model.ModelType);
            Assert.Equal("", model.ItemID);
            Assert.Empty(model.Elements);
            Assert.Empty(model.Textures);
            Assert.Empty(model.TextureImages);
            Assert.Empty(model.Displays);
            Assert.Empty(model.ModelFilePath);
        }

        [Fact]
        public void Constructor_WithValues_SetsTypeAndItemID()
        {
            using var model = new BlockModel("block", "minecraft:stone");

            Assert.Equal("block", model.ModelType);
            Assert.Equal("minecraft:stone", model.ItemID);
            Assert.Empty(model.Elements);
        }

        // ── AddElements ───────────────────────────────────────────────────────

        [Fact]
        public void AddElements_ParsesFromToRotationAndFaces()
        {
            using var model = new BlockModel();
            var elements = JsonSerializer.Deserialize<List<object>>(
                """
                [
                  {
                    "from": [0, 0, 0],
                    "to": [16, 8, 16],
                    "rotation": { "origin": [8, 8, 8], "axis": "y", "angle": 45 },
                    "faces": { "up": { "texture": "#top" } }
                  },
                  { "from": [4, 4, 4], "to": [12, 12, 12] }
                ]
                """)!;

            model.AddElements(elements);

            Assert.Equal(2, model.Elements.Count);
            Assert.Equal(new Point3D(16, 8, 16), model.Elements[0].To);
            Assert.Equal("y", model.Elements[0].Rotate.Axis);
            Assert.Equal("#top", model.Elements[0].Faces["up"].Texture);
            Assert.Equal(new Point3D(4, 4, 4), model.Elements[1].From);
            Assert.Empty(model.Elements[1].Faces);
        }

        [Fact]
        public void AddElements_CalledTwice_AppendsElements()
        {
            using var model = new BlockModel();
            var elements = JsonSerializer.Deserialize<List<object>>("""[{ "from": [0, 0, 0], "to": [1, 1, 1] }]""")!;

            model.AddElements(elements);
            model.AddElements(elements);

            Assert.Equal(2, model.Elements.Count);
        }

        // ── AddTextures ───────────────────────────────────────────────────────

        [Fact]
        public void AddTextures_NewAndExistingKeys_AddsOrOverwrites()
        {
            using var model = new BlockModel();
            model.AddTextures(new Dictionary<string, string> { ["all"] = "block/stone", ["particle"] = "#all" });

            model.AddTextures(new Dictionary<string, string> { ["all"] = "block/dirt", ["side"] = "block/grass" });

            Assert.Equal(3, model.Textures.Count);
            Assert.Equal("block/dirt", model.Textures["all"]);
            Assert.Equal("#all", model.Textures["particle"]);
            Assert.Equal("block/grass", model.Textures["side"]);
        }

        // ── AddTexureImages ───────────────────────────────────────────────────

        [Fact]
        public void AddTexureImages_AddsImage()
        {
            using var model = new BlockModel();
            var img = new Mat<Vec4b>(16, 16);

            model.AddTexureImages("layer", img);

            Assert.Same(img, model.TextureImages["layer"]);
        }

        [Fact]
        public void AddTexureImages_DuplicateKey_ThrowsArgumentException()
        {
            using var model = new BlockModel();
            model.AddTexureImages("layer", new Mat<Vec4b>(16, 16));

            using var duplicate = new Mat<Vec4b>(16, 16);
            Assert.Throws<ArgumentException>(() => model.AddTexureImages("layer", duplicate));
        }

        // ── AddDisplays ───────────────────────────────────────────────────────

        [Fact]
        public void AddDisplays_NewAndExistingKeys_AddsOrOverwrites()
        {
            using var model = new BlockModel();
            model.AddDisplays(JsonSerializer.Deserialize<Dictionary<string, object>>(
                """{ "gui": { "rotation": [30, 225, 0] }, "ground": { "scale": [0.25, 0.25, 0.25] } }""")!);

            model.AddDisplays(JsonSerializer.Deserialize<Dictionary<string, object>>(
                """{ "gui": { "rotation": [0, 0, 0] } }""")!);

            Assert.Equal(2, model.Displays.Count);
            Assert.Equal(new Point3D(0, 0, 0), model.Displays["gui"].Rotation);
            Assert.Equal(new Point3D(0.25, 0.25, 0.25), model.Displays["ground"].Scale);
        }

        // ── Dispose ───────────────────────────────────────────────────────────

        [Fact]
        public void Dispose_ClearsAllCollections()
        {
            var model = new BlockModel("block", "minecraft:stone");
            model.AddElements(JsonSerializer.Deserialize<List<object>>("""[{ "faces": { "up": {} } }]""")!);
            model.AddTextures(new Dictionary<string, string> { ["all"] = "block/stone" });
            var img = new Mat<Vec4b>(16, 16);
            model.AddTexureImages("all", img);
            model.AddDisplays(JsonSerializer.Deserialize<Dictionary<string, object>>("""{ "gui": {} }""")!);
            model.ModelFilePath.Add("assets/minecraft/models/block/stone.json");
            var element = model.Elements[0];

            model.Dispose();

            Assert.Empty(model.Elements);
            Assert.Empty(model.Textures);
            Assert.Empty(model.TextureImages);
            Assert.Empty(model.Displays);
            Assert.Empty(model.ModelFilePath);
            Assert.Empty(element.Faces);    // 要素も破棄される
            Assert.True(img.IsDisposed);    // テクスチャ画像も破棄される
        }

        [Fact]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var model = new BlockModel();
            model.Dispose();

            var exception = Record.Exception(model.Dispose);

            Assert.Null(exception);
        }
    }
}
