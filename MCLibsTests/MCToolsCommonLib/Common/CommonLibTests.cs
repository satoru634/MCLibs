using System.IO;
using System.Text;
using System.Text.Json;
using CsvHelper.Configuration;
using fNbt;
using MCLibsTests.Helpers;
using MCToolsCommonLib.BaseData;
using MCToolsCommonLib.Common;

namespace MCLibsTests.MCToolsCommonLib.Common
{
    /// <summary>
    /// CommonLib のテスト
    /// </summary>
    public class CommonLibTests : IDisposable
    {
        private readonly TempDirectory _temp = new TempDirectory();

        public void Dispose()
        {
            _temp.Dispose();
        }

        /// <summary>
        /// JSON 読み書きテスト用のデータ
        /// </summary>
        public class SampleData
        {
            public string Name { get; set; } = "";
            public int Value { get; set; } = 0;
        }

        /// <summary>
        /// CSV 読み書きテスト用のデータ
        /// </summary>
        public class CsvRow
        {
            public string Name { get; set; } = "";
            public int Value { get; set; } = 0;
        }

        /// <summary>
        /// CSV 読み書きテスト用のマッピング
        /// </summary>
        public sealed class CsvRowMap : ClassMap<CsvRow>
        {
            public CsvRowMap()
            {
                Map(m => m.Name).Name("name");
                Map(m => m.Value).Name("value");
            }
        }

        // ── ConvertUUID ───────────────────────────────────────────────────────

        [Fact]
        public void ConvertUUID_FourInts_ReturnsHyphenatedHexString()
        {
            var uuid = CommonLib.ConvertUUID([1, 2, 3, 4]);

            Assert.Equal("00000001-0000-0002-0000-000300000004", uuid);
        }

        [Fact]
        public void ConvertUUID_NegativeInts_TreatedAsUnsigned()
        {
            var uuid = CommonLib.ConvertUUID([-1, -1, -1, -1]);

            Assert.Equal("ffffffff-ffff-ffff-ffff-ffffffffffff", uuid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(5)]
        public void ConvertUUID_LengthNotFour_ReturnsEmpty(int length)
        {
            var uuid = CommonLib.ConvertUUID(new int[length]);

            Assert.Equal("", uuid);
        }

        // ── GetBoxCenter ──────────────────────────────────────────────────────

        [Fact]
        public void GetBoxCenter_ReturnsMidpoint()
        {
            var center = CommonLib.GetBoxCenter(new Point3D(0, 0, 0), new Point3D(10, 20, 30));

            Assert.Equal(5, center.X);
            Assert.Equal(10, center.Y);
            Assert.Equal(15, center.Z);
        }

        [Fact]
        public void GetBoxCenter_OddLength_TruncatesTowardStart()
        {
            var center = CommonLib.GetBoxCenter(new Point3D(-10, 1, 0), new Point3D(-5, 4, 5));

            Assert.Equal(-8, center.X);
            Assert.Equal(2, center.Y);
            Assert.Equal(2, center.Z);
        }

        // ── CalcDistance2D ────────────────────────────────────────────────────

        [Fact]
        public void CalcDistance2D_Point3DAndPoint3D_IgnoresY()
        {
            var dist = CommonLib.CalcDistance2D(new Point3D(0, 0, 0), new Point3D(3, 100, 4));

            Assert.Equal(5.0, dist, 9);
        }

        [Fact]
        public void CalcDistance2D_PointXZAndPointXZ_ReturnsEuclideanDistance()
        {
            var dist = CommonLib.CalcDistance2D(new PointXZ(-3, -4), new PointXZ(0, 0));

            Assert.Equal(5.0, dist, 9);
        }

        [Fact]
        public void CalcDistance2D_Point3DAndPointXZ_ReturnsEuclideanDistance()
        {
            var dist = CommonLib.CalcDistance2D(new Point3D(1, 50, 1), new PointXZ(4, 5));

            Assert.Equal(5.0, dist, 9);
        }

        [Fact]
        public void CalcDistance2D_PointXZAndPoint3D_ReturnsEuclideanDistance()
        {
            var dist = CommonLib.CalcDistance2D(new PointXZ(4, 5), new Point3D(1, 50, 1));

            Assert.Equal(5.0, dist, 9);
        }

        [Fact]
        public void CalcDistance2D_SamePoint_ReturnsZero()
        {
            var dist = CommonLib.CalcDistance2D(new PointXZ(7, 7), new PointXZ(7, 7));

            Assert.Equal(0.0, dist);
        }

        // ── GetWorldFolderList ────────────────────────────────────────────────

        [Fact]
        public void GetWorldFolderList_ReturnsOnlyFoldersWithLevelDat()
        {
            _temp.WriteText("", "saves", "WorldA", "level.dat");
            _temp.CreateDirectory("saves", "NotWorld");

            var worlds = CommonLib.GetWorldFolderList(_temp.Path);

            Assert.Single(worlds);
            Assert.Equal(_temp.Combine("saves", "WorldA"), worlds["WorldA"]);
        }

        [Fact]
        public void GetWorldFolderList_SavesNotExists_ThrowsDirectoryNotFoundException()
        {
            Assert.Throws<DirectoryNotFoundException>(() => CommonLib.GetWorldFolderList(_temp.Path));
        }

        // ── GetDimensionList ──────────────────────────────────────────────────

        [Fact]
        public void GetDimensionList_ConvertsFolderNamesToDimensionNames()
        {
            string world = _temp.CreateDirectory("MyWorld");
            _temp.CreateDirectory("MyWorld", "entities");
            _temp.CreateDirectory("MyWorld", "DIM-1", "entities");
            _temp.CreateDirectory("MyWorld", "DIM1", "entities");

            var dimensions = CommonLib.GetDimensionList(world, "MyWorld");

            Assert.Equal(3, dimensions.Count);
            Assert.Equal(_temp.Combine("MyWorld", "entities"), dimensions["overworld"]);
            Assert.Equal(_temp.Combine("MyWorld", "DIM-1", "entities"), dimensions["nether"]);
            Assert.Equal(_temp.Combine("MyWorld", "DIM1", "entities"), dimensions["end"]);
        }

        // ── ConvertDimensionName ──────────────────────────────────────────────

        [Theory]
        [InlineData("MyWorld", "overworld")]
        [InlineData("DIM-1", "nether")]
        [InlineData("DIM1", "end")]
        [InlineData("custom_dimension", "custom_dimension")]
        public void ConvertDimensionName_ReturnsExpectedName(string folderName, string expected)
        {
            Assert.Equal(expected, CommonLib.ConvertDimensionName(folderName, "MyWorld"));
        }

        // ── GetMinecraftProgramFiles ──────────────────────────────────────────

        [Fact]
        public void GetMinecraftProgramFiles_VersionsNotExists_ReturnsEmpty()
        {
            Assert.Empty(CommonLib.GetMinecraftProgramFiles(_temp.Path));
        }

        [Fact]
        public void GetMinecraftProgramFiles_NoLimit_ReturnsAllVersions()
        {
            CreateVersionJars("1.12.2", "1.20.1", "1.21.4");

            var versions = CommonLib.GetMinecraftProgramFiles(_temp.Path);

            Assert.Equal(["1.12.2", "1.20.1", "1.21.4"], versions.OrderBy(v => v));
        }

        [Fact]
        public void GetMinecraftProgramFiles_WithLimit_ExcludesOlderVersions()
        {
            CreateVersionJars("1.12.2", "1.13", "1.20.1");

            var versions = CommonLib.GetMinecraftProgramFiles(_temp.Path, "1.13");

            Assert.Equal(["1.13", "1.20.1"], versions.OrderBy(v => v));
        }

        [Fact]
        public void GetMinecraftProgramFiles_IgnoresNonJarFiles()
        {
            CreateVersionJars("1.20.1");
            _temp.WriteText("{}", "versions", "1.20.1", "1.20.1.json");

            var versions = CommonLib.GetMinecraftProgramFiles(_temp.Path);

            Assert.Equal(["1.20.1"], versions);
        }

        // ── GetMinecraftProgramFilePath ───────────────────────────────────────

        [Fact]
        public void GetMinecraftProgramFilePath_ExistingVersion_ReturnsJarPath()
        {
            CreateVersionJars("1.20.1", "1.21.4");

            var path = CommonLib.GetMinecraftProgramFilePath(_temp.Path, "1.21.4");

            Assert.Equal(_temp.Combine("versions", "1.21.4", "1.21.4.jar"), path);
        }

        [Fact]
        public void GetMinecraftProgramFilePath_UnknownVersion_ReturnsEmpty()
        {
            CreateVersionJars("1.20.1");

            Assert.Equal("", CommonLib.GetMinecraftProgramFilePath(_temp.Path, "1.99"));
        }

        [Fact]
        public void GetMinecraftProgramFilePath_VersionsNotExists_ReturnsEmpty()
        {
            Assert.Equal("", CommonLib.GetMinecraftProgramFilePath(_temp.Path, "1.20.1"));
        }

        // ── GetPlayerUUID ─────────────────────────────────────────────────────

        [Fact]
        public void GetPlayerUUID_ReadsUUIDFromLevelDat()
        {
            string levelPath = _temp.Combine("level.dat");
            var root = new NbtCompound("")
            {
                new NbtCompound("Data")
                {
                    new NbtCompound("Player")
                    {
                        new NbtIntArray("UUID", [1, 2, 3, 4]),
                    },
                },
            };
            new NbtFile(root).SaveToFile(levelPath, NbtCompression.GZip);

            var uuid = CommonLib.GetPlayerUUID(levelPath);

            Assert.Equal("00000001-0000-0002-0000-000300000004", uuid);
        }

        // ── ClampFloor ────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0.5, 10.0, 5.0)]    // 割合 × 最大値
        [InlineData(2.0, 10.0, 10.0)]   // 最大値でクランプ
        [InlineData(-1.0, 10.0, 0.0)]   // 0 でクランプ
        public void ClampFloor_Double_ReturnsClampedValue(double val, double maxVal, double expected)
        {
            Assert.Equal(expected, CommonLib.ClampFloor(val, maxVal), 9);
        }

        [Theory]
        [InlineData(1, 255, 255)]
        [InlineData(0, 255, 0)]
        [InlineData(-1, 255, 0)]
        public void ClampFloor_Int_ReturnsClampedValue(int val, int maxVal, int expected)
        {
            Assert.Equal(expected, CommonLib.ClampFloor(val, maxVal));
        }

        // ── GenerateDataTagCompound / GenerateDataTagList ─────────────────────

        [Fact]
        public void GenerateDataTagCompound_SupportedTypes_ReturnsDataTagString()
        {
            var compound = new NbtCompound("tag")
            {
                new NbtString("name", "abc"),
                new NbtInt("count", 3),
                new NbtCompound("sub") { new NbtInt("x", 1) },
                new NbtList("list") { new NbtCompound { new NbtInt("a", 1) }, new NbtCompound { new NbtInt("b", 2) } },
            };

            var tag = CommonLib.GenerateDataTagCompound(compound);

            Assert.Equal("{name:abc,count:3,sub:{x:1},list:[{a:1},{b:2}]}", tag);
        }

        [Fact]
        public void GenerateDataTagCompound_UnsupportedType_OutputsNameOnly()
        {
            var compound = new NbtCompound("tag") { new NbtByte("flag", 1) };

            Assert.Equal("{flag:}", CommonLib.GenerateDataTagCompound(compound));
        }

        [Fact]
        public void GenerateDataTagCompound_Empty_ReturnsEmptyBraces()
        {
            Assert.Equal("{}", CommonLib.GenerateDataTagCompound(new NbtCompound("tag")));
        }

        [Fact]
        public void GenerateDataTagList_Empty_ReturnsEmptyBrackets()
        {
            Assert.Equal("[]", CommonLib.GenerateDataTagList(new NbtList("list", NbtTagType.Compound)));
        }

        // ── GetItemInfo ───────────────────────────────────────────────────────

        [Fact]
        public void GetItemInfo_WithoutTag_ReturnsIdAndCount()
        {
            var item = new NbtCompound("item")
            {
                new NbtString("id", "minecraft:stone"),
                new NbtByte("Count", 32),
            };

            var (itemName, itemCount) = CommonLib.GetItemInfo(item);

            Assert.Equal("minecraft:stone", itemName);
            Assert.Equal(32, itemCount);
        }

        [Fact]
        public void GetItemInfo_WithTag_AppendsDataTagToId()
        {
            var item = new NbtCompound("item")
            {
                new NbtString("id", "minecraft:potion"),
                new NbtInt("Count", 1),
                new NbtCompound("tag") { new NbtString("Potion", "minecraft:healing") },
            };

            var (itemName, itemCount) = CommonLib.GetItemInfo(item);

            Assert.Equal("minecraft:potion{Potion:minecraft:healing}", itemName);
            Assert.Equal(1, itemCount);
        }

        // ── CompareVersion ────────────────────────────────────────────────────

        [Theory]
        [InlineData("1.21.4", "1.21.3", 1)]
        [InlineData("1.21.3", "1.21.4", -1)]
        [InlineData("1.21.3", "1.21.3", 0)]
        [InlineData("1.10", "1.9", 1)]          // 文字列比較ではなく数値比較
        [InlineData("1.21", "1.21.0", 0)]       // 足りない部分は 0 扱い
        [InlineData("1.21", "1.21.1", -1)]
        [InlineData("1.21.x", "1.21.0", 0)]     // 数値以外は 0 扱い
        [InlineData("", "", 0)]
        [InlineData("", "1.0", -1)]
        [InlineData("1.0", " ", 1)]
        public void CompareVersion_ReturnsExpectedResult(string versionA, string versionB, int expected)
        {
            Assert.Equal(expected, CommonLib.CompareVersion(versionA, versionB));
        }

        // ── IsEqual ───────────────────────────────────────────────────────────

        [Theory]
        [InlineData(1.0f, 1.0f, true)]
        [InlineData(1.0f, 1.0000005f, true)]
        [InlineData(1.0f, 1.00001f, false)]
        public void IsEqual_Float_ComparesWithTolerance(float a, float b, bool expected)
        {
            Assert.Equal(expected, CommonLib.IsEqual(a, b));
        }

        [Theory]
        [InlineData(0.0, 0.0, true)]
        [InlineData(0.1 + 0.2, 0.3, true)]
        [InlineData(1.0, 1.000001, false)]
        public void IsEqual_Double_ComparesWithTolerance(double a, double b, bool expected)
        {
            Assert.Equal(expected, CommonLib.IsEqual(a, b));
        }

        // ── GetAncestorDirectory ──────────────────────────────────────────────

        [Theory]
        [InlineData(0, @"C:\a\b\c\d.jar")]
        [InlineData(1, @"C:\a\b\c")]
        [InlineData(3, @"C:\a")]
        [InlineData(4, @"C:\")]
        [InlineData(10, "")]    // ルートより上は空文字
        public void GetAncestorDirectory_ReturnsAncestor(int level, string expected)
        {
            Assert.Equal(expected, CommonLib.GetAncestorDirectory(@"C:\a\b\c\d.jar", level));
        }

        // ── WriteCsv / ReadCsv ────────────────────────────────────────────────

        [Fact]
        public void WriteCsv_WritesHeaderAndRecords()
        {
            string path = _temp.Combine("out.csv");
            var rows = new List<CsvRow> { new() { Name = "a", Value = 1 }, new() { Name = "b", Value = 2 } };

            CommonLib.WriteCsv<CsvRow, CsvRowMap>(path, rows);

            var lines = File.ReadAllLines(path);
            Assert.Equal(["name,value", "a,1", "b,2"], lines);
        }

        [Fact]
        public void ReadCsv_WrittenByWriteCsv_ReturnsSameRecords()
        {
            string path = _temp.Combine("roundtrip.csv");
            var rows = new List<CsvRow> { new() { Name = "a", Value = 1 }, new() { Name = "b", Value = 2 } };
            CommonLib.WriteCsv<CsvRow, CsvRowMap>(path, rows);

            var loaded = CommonLib.ReadCsv<CsvRow, CsvRowMap>(path);

            Assert.Equal(2, loaded.Count);
            Assert.Equal("a", loaded[0].Name);
            Assert.Equal(2, loaded[1].Value);
        }

        [Fact]
        public void ReadCsv_UsesClassMapHeaderNames()
        {
            string path = _temp.WriteText("value,name\n10,x\n", "mapped.csv");

            var loaded = CommonLib.ReadCsv<CsvRow, CsvRowMap>(path);

            var row = Assert.Single(loaded);
            Assert.Equal("x", row.Name);
            Assert.Equal(10, row.Value);
        }

        [Fact]
        public void ReadCsv_HeaderOnly_ReturnsEmpty()
        {
            string path = _temp.WriteText("name,value\n", "empty.csv");

            Assert.Empty(CommonLib.ReadCsv<CsvRow, CsvRowMap>(path));
        }

        // ── ReadJson ──────────────────────────────────────────────────────────

        [Fact]
        public void ReadJson_ByPath_ValidFile_ReturnsDeserializedObject()
        {
            string path = _temp.WriteText("""{"Name":"hello","Value":42}""", "sample.json");

            var result = CommonLib.ReadJson<SampleData>(path);

            Assert.Equal("hello", result.Name);
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void ReadJson_ByPath_FileNotFound_ThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => CommonLib.ReadJson<SampleData>(_temp.Combine("none.json")));
        }

        [Fact]
        public void ReadJson_ByStream_ValidStream_ReturnsDeserializedObject()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("""{"Name":"stream","Value":7}"""));

            var result = CommonLib.ReadJson<SampleData>(stream);

            Assert.Equal("stream", result.Name);
            Assert.Equal(7, result.Value);
        }

        // ── DeserializeJson ───────────────────────────────────────────────────

        [Fact]
        public void DeserializeJson_ValidJson_ReturnsObject()
        {
            var result = CommonLib.DeserializeJson<SampleData>("""{"Name":"test","Value":1}""");

            Assert.Equal("test", result.Name);
            Assert.Equal(1, result.Value);
        }

        [Fact]
        public void DeserializeJson_NullInput_ReturnsNewInstance()
        {
            var result = CommonLib.DeserializeJson<SampleData>(null);

            Assert.Equal("", result.Name);
            Assert.Equal(0, result.Value);
        }

        [Fact]
        public void DeserializeJson_JsonNullLiteral_ReturnsNewInstance()
        {
            var result = CommonLib.DeserializeJson<SampleData>("null");

            Assert.NotNull(result);
            Assert.Equal("", result.Name);
        }

        [Fact]
        public void DeserializeJson_InvalidJson_ThrowsJsonException()
        {
            Assert.Throws<JsonException>(() => CommonLib.DeserializeJson<SampleData>("invalid json"));
        }

        // ── WriteJson ─────────────────────────────────────────────────────────

        [Fact]
        public void WriteJson_DirectoryNotExists_CreatesDirectoryAndFile()
        {
            string path = _temp.Combine("sub", "nested", "out.json");

            CommonLib.WriteJson(path, new SampleData { Name = "dir", Value = 1 });

            Assert.True(File.Exists(path));
        }

        [Fact]
        public void WriteJson_JapaneseCharacters_NotUnicodeEscaped()
        {
            string path = _temp.Combine("japanese.json");

            CommonLib.WriteJson(path, new SampleData { Name = "日本語テスト" });

            var content = File.ReadAllText(path, Encoding.UTF8);
            Assert.Contains("日本語テスト", content);
            Assert.DoesNotContain("\\u", content);
        }

        [Fact]
        public void WriteJson_OverwritesExistingFile()
        {
            string path = _temp.Combine("overwrite.json");
            CommonLib.WriteJson(path, new SampleData { Name = "old", Value = 1 });

            CommonLib.WriteJson(path, new SampleData { Name = "new", Value = 2 });

            var loaded = CommonLib.ReadJson<SampleData>(path);
            Assert.Equal("new", loaded.Name);
            Assert.Equal(2, loaded.Value);
        }

        [Fact]
        public void WriteJson_ThenReadJson_RoundTrip()
        {
            string path = _temp.Combine("roundtrip.json");

            CommonLib.WriteJson(path, new SampleData { Name = "round", Value = 123 });
            var loaded = CommonLib.ReadJson<SampleData>(path);

            Assert.Equal("round", loaded.Name);
            Assert.Equal(123, loaded.Value);
        }

        /// <summary>
        /// versions/&lt;バージョン&gt;/&lt;バージョン&gt;.jar を作成する。
        /// </summary>
        private void CreateVersionJars(params string[] versions)
        {
            foreach (string version in versions)
            {
                _temp.WriteText("", "versions", version, $"{version}.jar");
            }
        }
    }
}
