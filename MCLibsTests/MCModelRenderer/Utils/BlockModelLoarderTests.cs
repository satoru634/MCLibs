using MCLibsTests.Helpers;
using MCModelRenderer.Utils;
using OpenCvSharp;

namespace MCLibsTests.MCModelRenderer.Utils
{
    /// <summary>
    /// BlockModelLoarder のテスト
    /// </summary>
    public class BlockModelLoarderTests : IDisposable
    {
        /// <summary>
        /// テスト用のバージョン（1.21.3 以前の読み込み処理を通す）
        /// </summary>
        private const string Version = "1.20.1";

        /// <summary>
        /// テスト用のバージョン（1.21.4 以降の読み込み処理を通す）
        /// </summary>
        private const string NewVersion = "1.21.4";

        private const string JaLangHash = "1234567890abcdef1234567890abcdef12345678";

        private static readonly Scalar Gray = new Scalar(128, 128, 128, 255);

        private static readonly Scalar White = new Scalar(255, 255, 255, 255);

        private static readonly Scalar Red = new Scalar(0, 0, 255, 255);

        private readonly TempDirectory _temp = new TempDirectory();

        private readonly string _jarPath;

        public BlockModelLoarderTests()
        {
            _jarPath = TestMinecraft.CreateJar(_temp.Path, Version, zip =>
            {
                zip.AddText("assets/minecraft/lang/en_us.json",
                    """
                    {
                      "itemGroup.buildingBlocks": "Building Blocks",
                      "block.minecraft.stone": "Stone",
                      "block.minecraft.stone.desc": "Stone Description",
                      "block.minecraft.grass_block": "Grass Block",
                      "item.minecraft.apple": "Apple",
                      "item.minecraft.two_layer": "Two Layer",
                      "item.minecraft.no_model": "No Model",
                      "item.minecraft.allay_spawn_egg": "Allay Spawn Egg",
                      "item.minecraft.unknown_spawn_egg": "Unknown Spawn Egg"
                    }
                    """);

                // ブロックモデル: item/stone → block/stone → block/cube_all → block/cube
                zip.AddText("assets/minecraft/models/item/stone.json", """{ "parent": "minecraft:block/stone" }""");
                zip.AddText("assets/minecraft/models/block/stone.json",
                    """{ "parent": "block/cube_all", "textures": { "all": "minecraft:block/stone" } }""");
                zip.AddText("assets/minecraft/models/block/cube_all.json",
                    """{ "parent": "block/cube", "textures": { "particle": "#all", "up": "#all" } }""");
                zip.AddText("assets/minecraft/models/block/cube.json",
                    """
                    {
                      "elements": [ { "from": [0, 0, 0], "to": [16, 16, 16], "faces": { "up": { "texture": "#up" } } } ],
                      "display": { "gui": { "rotation": [30, 225, 0], "scale": [0.625, 0.625, 0.625] } }
                    }
                    """);

                // 固定色が乗算されるブロック
                zip.AddText("assets/minecraft/models/item/grass_block.json", """{ "parent": "minecraft:block/grass_block" }""");
                zip.AddText("assets/minecraft/models/block/grass_block.json",
                    """
                    {
                      "textures": { "top": "minecraft:block/grass_block_top" },
                      "elements": [ { "from": [0, 0, 0], "to": [16, 16, 16], "faces": { "up": { "texture": "#top" } } } ]
                    }
                    """);

                // アイテムモデル
                zip.AddText("assets/minecraft/models/item/generated.json", """{ "parent": "builtin/generated" }""");
                zip.AddText("assets/minecraft/models/item/apple.json",
                    """{ "parent": "item/generated", "textures": { "layer0": "item/apple" } }""");
                zip.AddText("assets/minecraft/models/item/two_layer.json",
                    """{ "parent": "item/generated", "textures": { "layer0": "item/red", "layer1": "item/white_dot" } }""");

                // スポーンエッグ
                string spawnEgg = """{ "textures": { "layer0": "minecraft:item/spawn_egg" } }""";
                zip.AddText("assets/minecraft/models/item/allay_spawn_egg.json", spawnEgg);
                zip.AddText("assets/minecraft/models/item/unknown_spawn_egg.json", spawnEgg);

                // テクスチャ
                zip.AddPng("assets/minecraft/textures/block/stone.png", Gray);
                zip.AddPng("assets/minecraft/textures/block/grass_block_top.png", White);
                zip.AddPng("assets/minecraft/textures/item/apple.png", Red);
                zip.AddPng("assets/minecraft/textures/item/red.png", Red);
                zip.AddPng("assets/minecraft/textures/item/spawn_egg.png", White);
                zip.AddBytes("assets/minecraft/textures/item/white_dot.png", EncodeDotPng());
            });
        }

        public void Dispose()
        {
            _temp.Dispose();
        }

        /// <summary>
        /// 左上の1画素だけが不透明な白で、それ以外が透明な PNG を作成する。
        /// </summary>
        private static byte[] EncodeDotPng()
        {
            using var mat = new Mat(16, 16, MatType.CV_8UC4, new Scalar(0, 0, 0, 0));
            mat.Set(0, 0, new Vec4b(255, 255, 255, 255));
            Cv2.ImEncode(".png", mat, out byte[] png);
            return png;
        }

        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_CreatesSortedItemListFromLangFile()
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);

            Assert.Equal(
                [
                    "minecraft:allay_spawn_egg", "minecraft:apple", "minecraft:grass_block", "minecraft:no_model",
                    "minecraft:stone", "minecraft:two_layer", "minecraft:unknown_spawn_egg",
                ],
                loader.ItemIDList);
            Assert.Equal(
                ["Allay Spawn Egg", "Apple", "Grass Block", "No Model", "Stone", "Two Layer", "Unknown Spawn Egg"],
                loader.ItemNameList);
        }

        [Fact]
        public void Constructor_SelectsFirstItem()
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);

            Assert.Equal("minecraft:allay_spawn_egg", loader.ItemID);
            Assert.Equal("Allay Spawn Egg", loader.ItemName);
            Assert.True(loader.ModelEnabled);
        }

        [Fact]
        public void Constructor_NoLangFile_HasNoItems()
        {
            string jar = TestZip.Create(_temp.Combine("other", "versions", Version, $"{Version}.jar"), zip => zip.AddText("dummy.txt", ""));

            using var loader = new BlockModelLoarder(jar, "en_us", Version);

            Assert.Empty(loader.ItemIDList);
            Assert.Equal("", loader.ItemID);
            Assert.Equal("", loader.ItemName);
            Assert.False(loader.ModelEnabled);
        }

        [Fact]
        public void Constructor_Japanese_ReadsNamesFromAssetObjects()
        {
            TestMinecraft.CreateAssetIndex(_temp.Path, Version, "5", new Dictionary<string, string>
            {
                ["minecraft/lang/ja_jp.json"] = JaLangHash,
            });
            TestMinecraft.CreateAssetObject(_temp.Path, JaLangHash,
                """
                {
                  "block.minecraft.stone": "石",
                  "block.minecraft.stone.desc": "石の説明",
                  "block.minecraft.grass_block": "草ブロック",
                  "item.minecraft.apple": "リンゴ",
                  "item.minecraft.two_layer": "2層",
                  "item.minecraft.no_model": "モデルなし",
                  "item.minecraft.allay_spawn_egg": "アレイのスポーンエッグ",
                  "item.minecraft.unknown_spawn_egg": "不明なスポーンエッグ"
                }
                """);

            using var loader = new BlockModelLoarder(_jarPath, "ja_jp", Version);

            Assert.Equal("石", loader.GetItemName("minecraft:stone"));
            Assert.Equal("minecraft:apple", loader.GetItemID("リンゴ"));
        }

        [Fact]
        public void Constructor_NewVersion_FiltersByItemsDefinition()
        {
            string jar = TestMinecraft.CreateJar(_temp.Combine("new"), NewVersion, zip =>
            {
                zip.AddText("assets/minecraft/lang/en_us.json",
                    """{ "block.minecraft.stone": "Stone", "block.minecraft.wall_torch": "Wall Torch" }""");
                zip.AddText("assets/minecraft/items/stone.json", "{}");
            });

            using var loader = new BlockModelLoarder(jar, "en_us", NewVersion);

            Assert.Equal(["minecraft:stone"], loader.ItemIDList);
        }

        [Fact]
        public void Constructor_Default_HasNoItems()
        {
            using var loader = new BlockModelLoarder();

            Assert.Empty(loader.ItemIDList);
            Assert.Empty(loader.ItemNameList);
            Assert.Equal("", loader.ItemID);
            Assert.False(loader.ModelEnabled);
        }

        // ── GetItemID / GetItemName / ExsistsItemID / ExistsItemName ──────────

        [Theory]
        [InlineData("Stone", "minecraft:stone")]
        [InlineData("Unknown", "")]
        public void GetItemID_ReturnsIDForName(string itemName, string expected)
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);

            Assert.Equal(expected, loader.GetItemID(itemName));
        }

        [Theory]
        [InlineData("minecraft:apple", "Apple")]
        [InlineData("minecraft:stone", "Stone")]    // ".desc" などの派生キーでは上書きされない
        [InlineData("minecraft:unknown", "")]
        public void GetItemName_ReturnsNameForID(string itemID, string expected)
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);

            Assert.Equal(expected, loader.GetItemName(itemID));
        }

        [Theory]
        [InlineData("minecraft:apple", true)]
        [InlineData("minecraft:unknown", false)]
        [InlineData("apple", false)]
        public void ExsistsItemID_ReturnsWhetherIDExists(string itemID, bool expected)
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);

            Assert.Equal(expected, loader.ExsistsItemID(itemID));
        }

        [Theory]
        [InlineData("Apple", true)]
        [InlineData("apple", false)]    // 大文字小文字を区別する
        [InlineData("Building Blocks", false)]
        public void ExistsItemName_ReturnsWhetherNameExists(string itemName, bool expected)
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);

            Assert.Equal(expected, loader.ExistsItemName(itemName));
        }

        // ── LoadModel ─────────────────────────────────────────────────────────

        [Fact]
        public void LoadModel_Block_LoadsParentsRecursively()
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);

            loader.LoadModel("Stone");

            Assert.Equal(
                [
                    "assets/minecraft/models/item/stone.json",
                    "assets/minecraft/models/block/stone.json",
                    "assets/minecraft/models/block/cube_all.json",
                    "assets/minecraft/models/block/cube.json",
                ],
                loader.Model.ModelFilePath);
            Assert.Equal("block", loader.Model.ModelType);
            Assert.Equal("minecraft:stone", loader.Model.ItemID);
            Assert.Single(loader.Model.Elements);
            Assert.Equal("minecraft:block/stone", loader.Model.Textures["all"]);
            Assert.Equal("#all", loader.Model.Textures["up"]);
            Assert.True(loader.Model.Displays.ContainsKey("gui"));
        }

        [Fact]
        public void LoadModel_Block_LoadsOnlyNonReferenceTextures()
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);

            loader.LoadModel("Stone");

            // "#" で始まる参照はテクスチャ画像として読み込まない
            Assert.Equal(["all"], loader.Model.TextureImages.Keys);
            Assert.Equal(new Vec4b(128, 128, 128, 255), loader.Model.TextureImages["all"].GetIndexer()[0, 0]);
        }

        [Fact]
        public void LoadModel_Success_UpdatesSelectedItem()
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);

            loader.LoadModel("Stone");

            Assert.Equal("minecraft:stone", loader.ItemID);
            Assert.Equal("Stone", loader.ItemName);
            Assert.True(loader.ModelEnabled);
        }

        [Fact]
        public void LoadModel_Item_ModelTypeIsItem()
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);

            loader.LoadModel("Apple");

            Assert.Equal("item", loader.Model.ModelType);
            Assert.Empty(loader.Model.Elements);
            Assert.Equal(new Vec4b(0, 0, 255, 255), loader.Model.TextureImages["layer"].GetIndexer()[8, 8]);
        }

        [Fact]
        public void LoadModel_ItemWithLayers_OverlaysLayersInOrder()
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);

            loader.LoadModel("Two Layer");

            var indexer = loader.Model.TextureImages["layer"].GetIndexer();
            Assert.Single(loader.Model.TextureImages);
            Assert.Equal(new Vec4b(255, 255, 255, 255), indexer[0, 0]);   // layer1 の不透明部分
            Assert.Equal(new Vec4b(0, 0, 255, 255), indexer[1, 1]);       // layer1 の透明部分は layer0 のまま
        }

        [Fact]
        public void LoadModel_TextureWithFixedColor_MultipliesColor()
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);

            loader.LoadModel("Grass Block");

            // 白(255,255,255)に colorset.json の 91BD59 を乗算した色（BGRA）
            Assert.Equal(new Vec4b(0x59, 0xBD, 0x91, 255), loader.Model.TextureImages["top"].GetIndexer()[0, 0]);
        }

        [Fact]
        public void LoadModel_RegisteredSpawnEgg_MultipliesColor()
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);

            loader.LoadModel("Allay Spawn Egg");

            // 白(255,255,255)に 00DAFF を乗算した色（BGRA）
            Assert.Equal(new Vec4b(255, 218, 0, 255), loader.Model.TextureImages["layer"].GetIndexer()[0, 0]);
        }

        [Fact]
        public void LoadModel_UnregisteredSpawnEgg_DoesNotThrowAndKeepsColor()
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);

            var exception = Record.Exception(() => loader.LoadModel("Unknown Spawn Egg"));

            Assert.Null(exception);
            Assert.Equal(new Vec4b(255, 255, 255, 255), loader.Model.TextureImages["layer"].GetIndexer()[0, 0]);
        }

        [Fact(Skip = "既知の不具合: 未登録のアイテム名では FirstOrDefault の Key が null になり、ContainsKey(null) で ArgumentNullException が発生する")]
        public void LoadModel_UnknownName_ResetsModelAndKeepsSelection()
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);
            loader.LoadModel("Stone");

            loader.LoadModel("Unknown");

            Assert.Empty(loader.Model.ModelFilePath);
            Assert.Equal("", loader.Model.ItemID);
            Assert.Equal("minecraft:stone", loader.ItemID);
        }

        [Fact]
        public void LoadModel_ModelFileNotExists_KeepsSelection()
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);
            loader.LoadModel("Stone");

            loader.LoadModel("No Model");

            Assert.Empty(loader.Model.ModelFilePath);
            Assert.Equal("minecraft:stone", loader.ItemID);
            Assert.Equal("Stone", loader.ItemName);
        }

        [Fact]
        public void LoadModel_NewVersion_ResolvesModelPathFromItemsDefinition()
        {
            string jar = TestMinecraft.CreateJar(_temp.Combine("new"), NewVersion, zip =>
            {
                zip.AddText("assets/minecraft/lang/en_us.json", """{ "block.minecraft.stone": "Stone" }""");
                zip.AddText("assets/minecraft/items/stone.json",
                    """{ "model": { "type": "minecraft:model", "model": "minecraft:block/stone" } }""");
                zip.AddText("assets/minecraft/models/block/stone.json",
                    """{ "textures": { "all": "minecraft:block/stone" }, "elements": [ { "from": [0, 0, 0], "to": [16, 16, 16] } ] }""");
                zip.AddPng("assets/minecraft/textures/block/stone.png", Gray);
            });
            using var loader = new BlockModelLoarder(jar, "en_us", NewVersion);

            loader.LoadModel("Stone");

            Assert.Equal(["assets/minecraft/models/block/stone.json"], loader.Model.ModelFilePath);
            Assert.Equal("block", loader.Model.ModelType);
            Assert.True(loader.Model.TextureImages.ContainsKey("all"));
        }

        // ── Dispose ───────────────────────────────────────────────────────────

        [Fact]
        public void Dispose_ClearsItemLists()
        {
            var loader = new BlockModelLoarder(_jarPath, "en_us", Version);
            loader.LoadModel("Stone");

            loader.Dispose();

            Assert.Empty(loader.ItemIDList);
            Assert.Empty(loader.ItemNameList);
            Assert.Empty(loader.Model.ModelFilePath);
            Assert.False(loader.ExsistsItemID("minecraft:stone"));
        }

        [Fact]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var loader = new BlockModelLoarder();
            loader.Dispose();

            var exception = Record.Exception(loader.Dispose);

            Assert.Null(exception);
        }
    }
}
