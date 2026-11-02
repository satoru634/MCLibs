using System.IO;
using MCLibsTests.Helpers;
using MCToolsCommonLib.Util;
using OpenCvSharp;

namespace MCLibsTests.MCToolsCommonLib.Util
{
    /// <summary>
    /// ResourcePackInfo のテスト
    /// </summary>
    /// <remarks>
    /// ResourcePackInfo の名前空間は MCToolsCommonLib.Util（Utils ではない）のため、テストも Util 配下に置く。
    /// </remarks>
    public class ResourcePackInfoTests : IDisposable
    {
        private const string StonePath = "minecraft/textures/block/stone.png";

        private const string ApplePath = "minecraft/textures/item/apple.png";

        private readonly TempDirectory _temp = new TempDirectory();

        private readonly string _basePath;

        public ResourcePackInfoTests()
        {
            _basePath = _temp.CreateDirectory("resourcepacks");
        }

        public void Dispose()
        {
            _temp.Dispose();
        }

        /// <summary>
        /// ディレクトリ形式のリソースパックを作成する。
        /// </summary>
        private string CreateDirectoryPack(string packName, string resourcePath, Scalar bgra)
        {
            string path = Path.Combine(_basePath, packName, "assets", resourcePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, TestZip.EncodePng(bgra));
            return path;
        }

        /// <summary>
        /// zip 形式のリソースパックを作成する。
        /// </summary>
        private string CreateZipPack(string zipName, string nameSpace, string? resourcePath, Scalar bgra)
        {
            return TestZip.Create(Path.Combine(_basePath, zipName), zip =>
            {
                zip.AddText("pack.mcmeta", "{}");
                zip.AddDirectory($"assets/{nameSpace}/");
                if (resourcePath != null)
                {
                    zip.AddPng($"assets/{resourcePath}", bgra);
                }
            });
        }

        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_DataIsEmpty()
        {
            var info = new ResourcePackInfo(_basePath);

            Assert.Empty(info.Data);
        }

        // ── GetList ───────────────────────────────────────────────────────────

        [Fact]
        public void GetList_DirectoryPack_AddsInfoPerNameSpace()
        {
            CreateDirectoryPack("PackA", StonePath, new Scalar(0, 0, 0, 255));
            Directory.CreateDirectory(Path.Combine(_basePath, "PackA", "assets", "examplemod"));
            var info = new ResourcePackInfo(_basePath);

            info.GetList();

            Assert.Equal(2, info.Data.Count);
            Assert.All(info.Data, d =>
            {
                Assert.Equal("PackA", d.ResourcePackName);
                Assert.Equal("dir", d.Type);
                Assert.Equal(Path.Combine(_basePath, "PackA", "assets"), d.AssetsDirPath);
            });
            Assert.Equal(["examplemod", "minecraft"], info.Data.Select(d => d.NameSpace).OrderBy(n => n));
        }

        [Fact]
        public void GetList_ZipPack_AddsInfoFromNameSpaceDirectoryEntry()
        {
            string zipPath = CreateZipPack("PackB.zip", "minecraft", ApplePath, new Scalar(0, 0, 0, 255));
            var info = new ResourcePackInfo(_basePath);

            info.GetList();

            var data = Assert.Single(info.Data);
            Assert.Equal("PackB.zip", data.ResourcePackName);
            Assert.Equal("zip", data.Type);
            Assert.Equal("minecraft", data.NameSpace);
            Assert.Equal(zipPath, data.AssetsDirPath);
        }

        // ── SearchResource / ExistsResource ───────────────────────────────────

        [Fact]
        public void SearchResource_DirectoryPack_ReturnsFilePathAndType()
        {
            string stone = CreateDirectoryPack("PackA", StonePath, new Scalar(0, 0, 0, 255));
            var info = new ResourcePackInfo(_basePath);
            info.GetList();

            var (path, type) = info.SearchResource("minecraft", StonePath);

            Assert.Equal(stone, path);
            Assert.Equal("dir", type);
        }

        [Fact]
        public void SearchResource_ZipPack_ReturnsZipPathAndType()
        {
            string zipPath = CreateZipPack("PackB.zip", "minecraft", ApplePath, new Scalar(0, 0, 0, 255));
            var info = new ResourcePackInfo(_basePath);
            info.GetList();

            var (path, type) = info.SearchResource("minecraft", ApplePath);

            Assert.Equal(zipPath, path);
            Assert.Equal("zip", type);
        }

        [Fact]
        public void SearchResource_NotFound_ReturnsEmpty()
        {
            CreateDirectoryPack("PackA", StonePath, new Scalar(0, 0, 0, 255));
            var info = new ResourcePackInfo(_basePath);
            info.GetList();

            var (path, type) = info.SearchResource("minecraft", "minecraft/textures/block/none.png");

            Assert.Equal("", path);
            Assert.Equal("", type);
        }

        [Fact]
        public void SearchResource_OtherNameSpace_ReturnsEmpty()
        {
            CreateDirectoryPack("PackA", StonePath, new Scalar(0, 0, 0, 255));
            var info = new ResourcePackInfo(_basePath);
            info.GetList();

            var (path, _) = info.SearchResource("examplemod", StonePath);

            Assert.Equal("", path);
        }

        [Fact]
        public void SearchResource_ResourceInSecondZip_ReturnsSecondZip()
        {
            CreateZipPack("PackA.zip", "minecraft", null, new Scalar(0, 0, 0, 255));
            string second = CreateZipPack("PackB.zip", "minecraft", ApplePath, new Scalar(0, 0, 0, 255));
            var info = new ResourcePackInfo(_basePath);
            info.GetList();

            var (path, _) = info.SearchResource("minecraft", ApplePath);

            Assert.Equal(second, path);
        }

        [Fact]
        public void ExistsResource_ReturnsWhetherResourceExists()
        {
            CreateDirectoryPack("PackA", StonePath, new Scalar(0, 0, 0, 255));
            var info = new ResourcePackInfo(_basePath);
            info.GetList();

            Assert.True(info.ExistsResource("minecraft", StonePath));
            Assert.False(info.ExistsResource("minecraft", ApplePath));
        }

        // ── GetTextureData / GetTextureFromZip ────────────────────────────────

        [Fact]
        public void GetTextureData_DirectoryPack_LoadsImage()
        {
            CreateDirectoryPack("PackA", StonePath, new Scalar(1, 2, 3, 255));
            var info = new ResourcePackInfo(_basePath);
            info.GetList();
            using var tracker = new ResourcesTracker();

            var img = info.GetTextureData(tracker, StonePath, "minecraft");

            Assert.Equal(new Vec4b(1, 2, 3, 255), img.GetIndexer()[0, 0]);
        }

        [Fact]
        public void GetTextureData_ZipPack_LoadsImage()
        {
            CreateZipPack("PackB.zip", "minecraft", ApplePath, new Scalar(4, 5, 6, 128));
            var info = new ResourcePackInfo(_basePath);
            info.GetList();
            using var tracker = new ResourcesTracker();

            var img = info.GetTextureData(tracker, ApplePath, "minecraft");

            Assert.Equal(new Vec4b(4, 5, 6, 128), img.GetIndexer()[0, 0]);
        }

        [Fact]
        public void GetTextureFromZip_LoadsSpecifiedEntry()
        {
            string zipPath = CreateZipPack("PackB.zip", "minecraft", ApplePath, new Scalar(7, 8, 9, 255));
            var info = new ResourcePackInfo(_basePath);
            using var tracker = new ResourcesTracker();

            var img = info.GetTextureFromZip(tracker, zipPath, $"assets/{ApplePath}");

            Assert.Equal(16, img.Width);
            Assert.Equal(new Vec4b(7, 8, 9, 255), img.GetIndexer()[15, 15]);
        }
    }
}
