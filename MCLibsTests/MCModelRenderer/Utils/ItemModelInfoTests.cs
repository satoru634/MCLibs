using MCLibsTests.Helpers;
using MCModelRenderer.Utils;

namespace MCLibsTests.MCModelRenderer.Utils
{
    /// <summary>
    /// ItemModelInfo のテスト
    /// </summary>
    public class ItemModelInfoTests : IDisposable
    {
        /// <summary>
        /// テスト用のバージョン（1.21.4 以降の items 定義を使う）
        /// </summary>
        private const string Version = "1.21.4";

        private readonly TempDirectory _temp = new TempDirectory();

        private readonly string _jarPath;

        public ItemModelInfoTests()
        {
            _jarPath = TestMinecraft.CreateJar(_temp.Path, Version, zip =>
            {
                zip.AddText("assets/minecraft/items/stone.json",
                    """{ "model": { "type": "minecraft:model", "model": "minecraft:block/stone" } }""");
                zip.AddText("assets/minecraft/items/no_namespace.json",
                    """{ "model": { "type": "minecraft:model", "model": "item/no_namespace" } }""");
                zip.AddText("assets/minecraft/items/chest.json",
                    """{ "model": { "type": "minecraft:special", "base": "minecraft:item/chest", "model": { "type": "minecraft:chest" } } }""");
                zip.AddText("assets/minecraft/items/clock.json",
                    """
                    {
                      "model": {
                        "type": "minecraft:select",
                        "property": "minecraft:context_dimension",
                        "cases": [ { "when": "minecraft:overworld", "model": { "type": "minecraft:model", "model": "minecraft:item/clock_day" } } ],
                        "fallback": { "type": "minecraft:model", "model": "minecraft:item/clock_00" }
                      }
                    }
                    """);
                zip.AddText("assets/minecraft/items/fishing_rod.json",
                    """
                    {
                      "model": {
                        "type": "minecraft:condition",
                        "property": "minecraft:fishing_rod/cast",
                        "on_true": { "type": "minecraft:model", "model": "minecraft:item/fishing_rod_cast" },
                        "on_false": { "type": "minecraft:model", "model": "minecraft:item/fishing_rod" }
                      }
                    }
                    """);
                zip.AddText("assets/minecraft/items/bow.json",
                    """
                    {
                      "model": {
                        "type": "minecraft:range_dispatch",
                        "property": "minecraft:use_duration",
                        "entries": [
                          { "threshold": 0.65, "model": { "type": "minecraft:model", "model": "minecraft:item/bow_pulling_1" } },
                          { "threshold": 0.0, "model": { "type": "minecraft:model", "model": "minecraft:item/bow_pulling_0" } }
                        ]
                      }
                    }
                    """);
                zip.AddText("assets/minecraft/items/no_zero_threshold.json",
                    """
                    {
                      "model": {
                        "type": "minecraft:range_dispatch",
                        "entries": [ { "threshold": 0.5, "model": { "type": "minecraft:model", "model": "minecraft:item/x" } } ]
                      }
                    }
                    """);
                zip.AddText("assets/minecraft/items/nested.json",
                    """
                    {
                      "model": {
                        "type": "minecraft:select",
                        "fallback": {
                          "type": "minecraft:condition",
                          "on_false": { "type": "minecraft:special", "base": "minecraft:item/nested_base" }
                        }
                      }
                    }
                    """);
                zip.AddText("assets/minecraft/items/composite.json",
                    """{ "model": { "type": "minecraft:composite", "models": [] } }""");
                zip.AddText("assets/minecraft/items/no_model.json", "{}");
            });
        }

        public void Dispose()
        {
            _temp.Dispose();
        }

        /// <summary>
        /// 指定したアイテムの ItemModelInfo を作成し、モデルのパスを取得する。
        /// </summary>
        private string GetModelPath(string itemID)
        {
            using var info = new ItemModelInfo(itemID, _jarPath, "en_us", Version);
            return info.GetModelPath();
        }

        // ── GetModelPath ──────────────────────────────────────────────────────

        [Theory]
        [InlineData("minecraft:stone", "assets/minecraft/models/block/stone.json")]           // model
        [InlineData("minecraft:no_namespace", "assets/minecraft/models/item/no_namespace.json")] // 名前空間なし
        [InlineData("minecraft:chest", "assets/minecraft/models/item/chest.json")]            // special は base
        [InlineData("minecraft:clock", "assets/minecraft/models/item/clock_00.json")]         // select は fallback
        [InlineData("minecraft:fishing_rod", "assets/minecraft/models/item/fishing_rod.json")] // condition は on_false
        [InlineData("minecraft:bow", "assets/minecraft/models/item/bow_pulling_0.json")]      // range_dispatch は閾値 0
        [InlineData("minecraft:nested", "assets/minecraft/models/item/nested_base.json")]     // 入れ子
        public void GetModelPath_ResolvesModelPathByType(string itemID, string expected)
        {
            Assert.Equal(expected, GetModelPath(itemID));
        }

        [Theory]
        [InlineData("minecraft:no_zero_threshold")] // 閾値 0 のエントリがない
        [InlineData("minecraft:composite")]         // 未対応のタイプ
        [InlineData("minecraft:no_model")]          // model キーがない
        [InlineData("minecraft:not_exists")]        // items 定義がない
        public void GetModelPath_Unresolvable_ReturnsEmpty(string itemID)
        {
            Assert.Equal("", GetModelPath(itemID));
        }

        [Fact]
        public void GetModelPath_DefaultConstructor_ReturnsEmpty()
        {
            using var info = new ItemModelInfo();

            Assert.Equal("", info.GetModelPath());
        }

        // ── Dispose ───────────────────────────────────────────────────────────

        [Fact]
        public void Dispose_ClearsItemModel()
        {
            var info = new ItemModelInfo("minecraft:stone", _jarPath, "en_us", Version);

            info.Dispose();

            Assert.Equal("", info.GetModelPath());
        }
    }
}
