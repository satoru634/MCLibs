using System.IO;
using MCLibsTests.Helpers;
using MCToolsCommonLib.Common;

namespace MCLibsTests.MCToolsCommonLib.Common
{
    /// <summary>
    /// Setting のテスト
    /// </summary>
    public class SettingTests : IDisposable
    {
        private readonly TempDirectory _temp = new TempDirectory();

        public void Dispose()
        {
            _temp.Dispose();
        }

        /// <summary>
        /// テスト用の設定データ
        /// </summary>
        public class Config
        {
            public string MinecraftPath { get; set; } = "default";
            public int Size { get; set; } = 16;
        }

        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_ExistingFile_LoadsData()
        {
            string path = _temp.Combine("config.json");
            CommonLib.WriteJson(path, new Config { MinecraftPath = "loaded", Size = 64 });

            var setting = new Setting<Config>(path);

            Assert.Equal("loaded", setting.Data.MinecraftPath);
            Assert.Equal(64, setting.Data.Size);
        }

        [Fact]
        public void Constructor_FileNotExists_CreatesFileWithDefaultData()
        {
            string path = _temp.Combine("new_config.json");

            var setting = new Setting<Config>(path);

            Assert.True(File.Exists(path));
            Assert.Equal("default", setting.Data.MinecraftPath);
            Assert.Equal(16, setting.Data.Size);
        }

        [Fact]
        public void Constructor_OnLoadFalse_DoesNotCreateFile()
        {
            string path = _temp.Combine("no_load.json");

            var setting = new Setting<Config>(path, onLoad: false);

            Assert.False(File.Exists(path));
            Assert.Equal("default", setting.Data.MinecraftPath);
        }

        // ── Load ──────────────────────────────────────────────────────────────

        [Fact]
        public void Load_CalledAgain_ReloadsFromFile()
        {
            string path = _temp.Combine("reload.json");
            CommonLib.WriteJson(path, new Config { MinecraftPath = "first" });
            var setting = new Setting<Config>(path);

            CommonLib.WriteJson(path, new Config { MinecraftPath = "second" });
            setting.Load();

            Assert.Equal("second", setting.Data.MinecraftPath);
        }

        // ── Save ──────────────────────────────────────────────────────────────

        [Fact]
        public void Save_WritesCurrentData()
        {
            string path = _temp.Combine("save.json");
            var setting = new Setting<Config>(path, onLoad: false);
            setting.Data.MinecraftPath = "saved";
            setting.Data.Size = 128;

            setting.Save();

            var loaded = CommonLib.ReadJson<Config>(path);
            Assert.Equal("saved", loaded.MinecraftPath);
            Assert.Equal(128, loaded.Size);
        }

        [Fact]
        public void Save_ThenLoad_RoundTrip()
        {
            string path = _temp.Combine("roundtrip.json");
            var setting = new Setting<Config>(path, onLoad: false);
            setting.Data.MinecraftPath = "roundtrip";

            setting.Save();
            setting.Data = new Config();
            setting.Load();

            Assert.Equal("roundtrip", setting.Data.MinecraftPath);
        }
    }
}
