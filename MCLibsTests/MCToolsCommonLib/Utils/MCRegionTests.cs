using System.IO;
using fNbt;
using MCLibsTests.Helpers;
using MCToolsCommonLib.Utils;

namespace MCLibsTests.MCToolsCommonLib.Utils
{
    /// <summary>
    /// MCRegion のテスト
    /// </summary>
    public class MCRegionTests : IDisposable
    {
        /// <summary>
        /// リージョンファイルのセクターサイズ
        /// </summary>
        private const int SectorSize = 4096;

        private readonly TempDirectory _temp = new TempDirectory();

        public void Dispose()
        {
            _temp.Dispose();
        }

        /// <summary>
        /// テスト用のリージョンファイルを作成する。
        /// </summary>
        /// <param name="chunks">チャンク座標（リージョン内 0～31）・圧縮形式・NBT の組</param>
        /// <returns>作成したファイルのパス</returns>
        private string CreateRegionFile(params (int x, int z, byte compression, NbtCompound root)[] chunks)
        {
            using var region = new MemoryStream();
            region.Write(new byte[SectorSize * 2]);   // ヘッダー（位置テーブル＋タイムスタンプ）

            foreach (var chunk in chunks)
            {
                var nbtCompression = chunk.compression == 1 ? NbtCompression.GZip : NbtCompression.ZLib;
                byte[] data = new NbtFile(chunk.root).SaveToBuffer(nbtCompression);
                int sectorOffset = (int)(region.Length / SectorSize);
                int sectorCount = (data.Length + 5 + SectorSize - 1) / SectorSize;

                // チャンク本体: 長さ(4byte, BE) + 圧縮形式(1byte) + データ、セクター境界までゼロ埋め
                byte[] body = new byte[sectorCount * SectorSize];
                WriteBigEndian(body, 0, data.Length + 1, 4);
                body[4] = chunk.compression;
                data.CopyTo(body, 5);
                region.Seek(0, SeekOrigin.End);
                region.Write(body);

                // 位置テーブル: オフセット(3byte, BE) + セクター数(1byte)
                byte[] location = new byte[4];
                WriteBigEndian(location, 0, sectorOffset, 3);
                location[3] = (byte)sectorCount;
                region.Seek(4 * (chunk.x + chunk.z * 32), SeekOrigin.Begin);
                region.Write(location);
            }

            string path = _temp.Combine("r.0.0.mca");
            File.WriteAllBytes(path, region.ToArray());
            return path;
        }

        /// <summary>
        /// 値をビッグエンディアンで書き込む。
        /// </summary>
        private static void WriteBigEndian(byte[] buffer, int offset, int value, int length)
        {
            for (int i = 0; i < length; i++)
            {
                buffer[offset + i] = (byte)(value >> (8 * (length - 1 - i)));
            }
        }

        /// <summary>
        /// テスト用のチャンク NBT を作成する。
        /// </summary>
        private static NbtCompound CreateChunkNbt(int marker)
        {
            return new NbtCompound("") { new NbtInt("Marker", marker) };
        }

        // ── HeaderOffset ──────────────────────────────────────────────────────

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 0, 4)]
        [InlineData(0, 1, 128)]
        [InlineData(31, 31, 4092)]
        [InlineData(33, 34, 260)]   // リージョン内座標に折り返す
        public void HeaderOffset_ReturnsLocationTableOffset(int chunkX, int chunkZ, int expected)
        {
            using var region = new MCRegion();

            Assert.Equal(expected, region.HeaderOffset(chunkX, chunkZ));
        }

        [Fact(Skip = "既知の不具合: 負のチャンク座標で % が負の値を返すため、オフセットが負になる")]
        public void HeaderOffset_NegativeChunk_WrapsIntoRegion()
        {
            using var region = new MCRegion();

            // チャンク(-1, -1) はリージョン(-1, -1) 内の (31, 31)
            Assert.Equal(4092, region.HeaderOffset(-1, -1));
        }

        // ── GetChunkLocation ──────────────────────────────────────────────────

        [Fact]
        public void GetChunkLocation_ExistingChunk_ReturnsOffsetAndSectors()
        {
            string path = CreateRegionFile((1, 2, 2, CreateChunkNbt(1)));
            using var region = new MCRegion();
            region.ReadFile(path);

            var (offset, sectors) = region.GetChunkLocation(1, 2);

            Assert.Equal(2, offset);
            Assert.Equal(1, sectors);
        }

        [Fact]
        public void GetChunkLocation_EmptyChunk_ReturnsZero()
        {
            string path = CreateRegionFile((1, 2, 2, CreateChunkNbt(1)));
            using var region = new MCRegion();
            region.ReadFile(path);

            Assert.Equal((0, 0), region.GetChunkLocation(0, 0));
        }

        // ── GetChunkData ──────────────────────────────────────────────────────

        [Fact]
        public void GetChunkData_ZlibChunk_ReturnsNbt()
        {
            string path = CreateRegionFile((0, 0, 2, CreateChunkNbt(10)), (5, 3, 2, CreateChunkNbt(20)));
            using var region = new MCRegion();
            region.ReadFile(path);

            var first = region.GetChunkData(0, 0);
            var second = region.GetChunkData(5, 3);

            Assert.Equal(10, first?["Marker"].IntValue);
            Assert.Equal(20, second?["Marker"].IntValue);
        }

        [Fact]
        public void GetChunkData_EmptyChunk_ReturnsNull()
        {
            string path = CreateRegionFile((0, 0, 2, CreateChunkNbt(10)));
            using var region = new MCRegion();
            region.ReadFile(path);

            Assert.Null(region.GetChunkData(1, 1));
        }

        [Fact]
        public void GetChunkData_GzipChunk_ReturnsNull()
        {
            string path = CreateRegionFile((0, 0, 1, CreateChunkNbt(10)));
            using var region = new MCRegion();
            region.ReadFile(path);

            Assert.Null(region.GetChunkData(0, 0));
        }

        // ── ReadFile ──────────────────────────────────────────────────────────

        [Fact]
        public void ReadFile_FileNotFound_ThrowsFileNotFoundException()
        {
            using var region = new MCRegion();

            Assert.Throws<FileNotFoundException>(() => region.ReadFile(_temp.Combine("none.mca")));
        }

        // ── Dispose ───────────────────────────────────────────────────────────

        [Fact]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var region = new MCRegion();
            region.Dispose();

            var exception = Record.Exception(region.Dispose);

            Assert.Null(exception);
        }
    }
}
