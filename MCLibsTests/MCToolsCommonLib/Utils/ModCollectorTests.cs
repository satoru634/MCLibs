using MCLibsTests.Helpers;
using MCToolsCommonLib.Utils;

namespace MCLibsTests.MCToolsCommonLib.Utils
{
    /// <summary>
    /// ModCollector のテスト
    /// </summary>
    public class ModCollectorTests : IDisposable
    {
        private readonly TempDirectory _temp = new TempDirectory();

        public void Dispose()
        {
            _temp.Dispose();
        }

        /// <summary>
        /// mods.toml を持つ MOD の jar を作成する。
        /// </summary>
        private string CreateModJar(string fileName, string modId)
        {
            return TestZip.Create(_temp.Combine("mods", fileName), zip =>
            {
                zip.AddText("META-INF/mods.toml", $"""
                    modLoader="javafml"
                    [[mods]]
                    modId="{modId}"
                    """);
            });
        }

        // ── GetModPath ────────────────────────────────────────────────────────

        [Fact]
        public void GetModPath_KnownNameSpace_ReturnsJarPath()
        {
            string modA = CreateModJar("mod_a.jar", "moda");
            string modB = CreateModJar("mod_b.jar", "modb");
            using var collector = new ModCollector(_temp.Combine("mods"), "en_us");

            Assert.Equal(modA, collector.GetModPath("moda"));
            Assert.Equal(modB, collector.GetModPath("modb"));
        }

        [Fact]
        public void GetModPath_UnknownNameSpace_ReturnsEmpty()
        {
            CreateModJar("mod_a.jar", "moda");
            using var collector = new ModCollector(_temp.Combine("mods"), "en_us");

            Assert.Equal("", collector.GetModPath("unknown"));
        }

        [Fact]
        public void GetModPath_JarWithoutModsToml_RegisteredAsMinecraft()
        {
            string jar = TestZip.Create(_temp.Combine("mods", "plain.jar"), zip => zip.AddText("dummy.txt", ""));
            using var collector = new ModCollector(_temp.Combine("mods"), "en_us");

            Assert.Equal(jar, collector.GetModPath("minecraft"));
        }

        [Fact]
        public void GetModPath_IgnoresNonJarFiles()
        {
            _temp.WriteText("", "mods", "readme.txt");
            using var collector = new ModCollector(_temp.Combine("mods"), "en_us");

            Assert.Equal("", collector.GetModPath("minecraft"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("not_exists")]
        public void GetModPath_BasePathEmptyOrNotExists_ReturnsEmpty(string basePath)
        {
            using var collector = new ModCollector(basePath == "" ? "" : _temp.Combine(basePath), "en_us");

            Assert.Equal("", collector.GetModPath("minecraft"));
        }

        [Fact]
        public void GetModPath_DefaultConstructor_ReturnsEmpty()
        {
            using var collector = new ModCollector();

            Assert.Equal("", collector.GetModPath("minecraft"));
        }

        // ── Dispose ───────────────────────────────────────────────────────────

        [Fact]
        public void Dispose_ClearsModList()
        {
            CreateModJar("mod_a.jar", "moda");
            var collector = new ModCollector(_temp.Combine("mods"), "en_us");

            collector.Dispose();

            Assert.Equal("", collector.GetModPath("moda"));
        }

        [Fact]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var collector = new ModCollector();
            collector.Dispose();

            var exception = Record.Exception(collector.Dispose);

            Assert.Null(exception);
        }
    }
}
