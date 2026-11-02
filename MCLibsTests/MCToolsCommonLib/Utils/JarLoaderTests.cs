using System.IO;
using MCLibsTests.Helpers;
using MCToolsCommonLib.Utils;
using OpenCvSharp;

namespace MCLibsTests.MCToolsCommonLib.Utils
{
    /// <summary>
    /// JarLoader のテスト
    /// </summary>
    public class JarLoaderTests : IDisposable
    {
        private const string Version = "1.20.1";

        private const string LangHash = "abcdef0123456789abcdef0123456789abcdef01";

        private readonly TempDirectory _temp = new TempDirectory();

        private readonly string _jarPath;

        public JarLoaderTests()
        {
            _jarPath = TestMinecraft.CreateJar(_temp.Path, Version, zip =>
            {
                zip.AddText("assets/minecraft/lang/en_us.json", """{ "block.minecraft.stone": "Stone" }""");
                zip.AddText("assets/minecraft/items/stone.json", "{}");
                zip.AddText("assets/minecraft/items/apple.json", "{}");
                zip.AddPng("assets/minecraft/textures/block/bgra.png", new Scalar(10, 20, 30, 40));

                // 3チャンネル（BGR）の PNG
                using var bgr = new Mat(4, 4, MatType.CV_8UC3, new Scalar(50, 60, 70));
                Cv2.ImEncode(".png", bgr, out byte[] png);
                zip.AddBytes("assets/minecraft/textures/block/bgr.png", png);
            });

            TestMinecraft.CreateAssetIndex(_temp.Path, Version, "5", new Dictionary<string, string>
            {
                ["minecraft/sounds.json"] = "0000000000000000000000000000000000000000",
                ["minecraft/lang/ja_jp.json"] = LangHash,
            });
        }

        public void Dispose()
        {
            _temp.Dispose();
        }

        // ── GetMincraftPath ───────────────────────────────────────────────────

        [Fact]
        public void GetMincraftPath_ReturnsThreeLevelsAboveJar()
        {
            var loader = new JarLoader(_jarPath, "en_us", Version);

            Assert.Equal(_temp.Path, loader.GetMincraftPath());
        }

        [Fact]
        public void GetMincraftPath_DefaultConstructor_ReturnsEmpty()
        {
            Assert.Equal("", new JarLoader().GetMincraftPath());
        }

        // ── Exists ────────────────────────────────────────────────────────────

        [Theory]
        [InlineData("assets/minecraft/items/stone.json", true)]
        [InlineData("assets/minecraft/items/none.json", false)]
        [InlineData("assets/minecraft/items", false)]   // 完全一致のみ
        public void Exists_ReturnsWhetherEntryExists(string path, bool expected)
        {
            var loader = new JarLoader(_jarPath, "en_us", Version);

            Assert.Equal(expected, loader.Exists(path));
        }

        // ── GetNameSpace ──────────────────────────────────────────────────────

        [Fact]
        public void GetNameSpace_WithoutModsToml_ReturnsMinecraft()
        {
            var loader = new JarLoader(_jarPath, "en_us", Version);

            Assert.Equal("minecraft", loader.GetNameSpace());
        }

        [Fact]
        public void GetNameSpace_WithModsToml_ReturnsModId()
        {
            string modJar = TestZip.Create(_temp.Combine("mods", "example.jar"), zip =>
            {
                zip.AddText("META-INF/mods.toml",
                    """
                    modLoader="javafml"
                    [[mods]]
                    modId="examplemod"
                    """);
            });

            var loader = new JarLoader(modJar, "en_us");

            Assert.Equal("examplemod", loader.GetNameSpace());
        }

        // ── JsonLoad ──────────────────────────────────────────────────────────

        [Fact]
        public void JsonLoad_ExistingEntry_ReturnsDeserializedData()
        {
            var loader = new JarLoader(_jarPath, "en_us", Version);

            var lang = loader.JsonLoad<Dictionary<string, string>>("assets/minecraft/lang/en_us.json");

            Assert.Equal("Stone", lang["block.minecraft.stone"]);
        }

        [Fact]
        public void JsonLoad_MissingEntry_ReturnsNewInstance()
        {
            var loader = new JarLoader(_jarPath, "en_us", Version);

            var data = loader.JsonLoad<Dictionary<string, string>>("assets/minecraft/lang/none.json");

            Assert.Empty(data);
        }

        // ── GetFileList ───────────────────────────────────────────────────────

        [Fact]
        public void GetFileList_ReturnsRelativePathsUnderBasePath()
        {
            var loader = new JarLoader(_jarPath, "en_us", Version);

            var files = loader.GetFileList("assets/minecraft/items");

            Assert.Equal(["apple.json", "stone.json"], files.OrderBy(f => f));
        }

        [Fact]
        public void GetFileList_NoMatch_ReturnsEmpty()
        {
            var loader = new JarLoader(_jarPath, "en_us", Version);

            Assert.Empty(loader.GetFileList("assets/other"));
        }

        // ── GetAssetListPath ──────────────────────────────────────────────────

        [Fact]
        public void GetAssetListPath_ReturnsIndexPathFromVersionJson()
        {
            var loader = new JarLoader(_jarPath, "ja_jp", Version);

            Assert.Equal(_temp.Combine("assets", "indexes", "5.json"), loader.GetAssetListPath());
        }

        // ── LoadImage ─────────────────────────────────────────────────────────

        [Fact]
        public void LoadImage_BgraPng_KeepsPixelValues()
        {
            var loader = new JarLoader(_jarPath, "en_us", Version);

            using var img = loader.LoadImage("assets/minecraft/textures/block/bgra.png");

            Assert.Equal(16, img.Width);
            Assert.Equal(new Vec4b(10, 20, 30, 40), img.GetIndexer()[0, 0]);
        }

        [Fact]
        public void LoadImage_BgrPng_ConvertsToOpaqueBgra()
        {
            var loader = new JarLoader(_jarPath, "en_us", Version);

            using var img = loader.LoadImage("assets/minecraft/textures/block/bgr.png");

            Assert.Equal(4, img.Width);
            Assert.Equal(new Vec4b(50, 60, 70, 255), img.GetIndexer()[3, 3]);
        }

        [Fact]
        public void LoadImage_MissingEntry_ReturnsEmptyMat()
        {
            var loader = new JarLoader(_jarPath, "en_us", Version);

            using var img = loader.LoadImage("assets/minecraft/textures/none.png");

            Assert.True(img.Empty());
        }

        // ── GetResourceHash / GenerateResourcePath ────────────────────────────

        [Fact]
        public void GetResourceHash_MatchingLanguage_ReturnsHash()
        {
            var loader = new JarLoader(_jarPath, "ja_jp", Version);

            Assert.Equal(LangHash, loader.GetResourceHash());
        }

        [Fact]
        public void GetResourceHash_LanguageIsCaseInsensitive()
        {
            var loader = new JarLoader(_jarPath, "JA_JP", Version);

            Assert.Equal(LangHash, loader.GetResourceHash());
        }

        [Fact]
        public void GetResourceHash_UnknownLanguage_ReturnsEmpty()
        {
            var loader = new JarLoader(_jarPath, "zz_zz", Version);

            Assert.Equal("", loader.GetResourceHash());
        }

        [Fact]
        public void GenerateResourcePath_ReturnsObjectPathFromHash()
        {
            var loader = new JarLoader(_jarPath, "ja_jp", Version);

            Assert.Equal(_temp.Combine("assets", "objects", "ab", LangHash), loader.GenerateResourcePath());
        }
    }
}
