using System.IO;
using System.IO.Compression;
using System.Text;
using MCModelRenderer.Utils;
using OpenCvSharp;

namespace MCLibsTests.MCModelRenderer.Utils
{
    /// <summary>
    /// BlockModelLoarderのテスト
    /// </summary>
    public class BlockModelLoarderTests : IDisposable
    {
        /// <summary>
        /// テスト用のバージョン（1.21.3以前の読み込み処理を通す）
        /// </summary>
        private const string Version = "1.20.1";

        /// <summary>
        /// テスト用の一時ディレクトリ
        /// </summary>
        private readonly string _tempDir;

        /// <summary>
        /// テスト用jarファイルのパス
        /// </summary>
        private readonly string _jarPath;

        /// <summary>
        /// コンストラクタ。テスト用のjarファイルを作成する。
        /// </summary>
        public BlockModelLoarderTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "MCLibsTests_" + Guid.NewGuid().ToString("N"));
            string versionDir = Path.Combine(_tempDir, "versions", Version);
            Directory.CreateDirectory(versionDir);
            _jarPath = Path.Combine(versionDir, $"{Version}.jar");
            CreateTestJar(_jarPath);
        }

        /// <summary>
        /// 一時ディレクトリを削除する。
        /// </summary>
        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }

        /// <summary>
        /// colorset.jsonに登録されているスポーンエッグは、テクスチャに色が乗算されること。
        /// </summary>
        [Fact]
        public void LoadModel_登録済みのスポーンエッグ_テクスチャに色が乗算される()
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);

            loader.LoadModel("Allay Spawn Egg");

            // 白(255,255,255)に00DAFFを乗算した色（BGRA）になる
            var pixel = loader.Model.TextureImages["layer"].GetIndexer()[0, 0];
            Assert.Equal(new Vec4b(255, 218, 0, 255), pixel);
        }

        /// <summary>
        /// colorset.jsonに登録されていないスポーンエッグは、例外にならず色も乗算されないこと。
        /// </summary>
        [Fact]
        public void LoadModel_未登録のスポーンエッグ_例外にならず色が乗算されない()
        {
            using var loader = new BlockModelLoarder(_jarPath, "en_us", Version);

            var exception = Record.Exception(() => loader.LoadModel("Unknown Spawn Egg"));

            Assert.Null(exception);
            var pixel = loader.Model.TextureImages["layer"].GetIndexer()[0, 0];
            Assert.Equal(new Vec4b(255, 255, 255, 255), pixel);
        }

        /// <summary>
        /// スポーンエッグ2種類を含むテスト用jarファイルを作成する。
        /// </summary>
        /// <param name="jarPath">作成するjarファイルのパス</param>
        private static void CreateTestJar(string jarPath)
        {
            using var zip = ZipFile.Open(jarPath, ZipArchiveMode.Create);

            AddText(zip, "assets/minecraft/lang/en_us.json",
                """
                {
                  "item.minecraft.allay_spawn_egg": "Allay Spawn Egg",
                  "item.minecraft.unknown_spawn_egg": "Unknown Spawn Egg"
                }
                """);

            string model = """{ "textures": { "layer0": "minecraft:item/spawn_egg" } }""";
            AddText(zip, "assets/minecraft/models/item/allay_spawn_egg.json", model);
            AddText(zip, "assets/minecraft/models/item/unknown_spawn_egg.json", model);

            // 真っ白なテクスチャ
            using var texture = new Mat(16, 16, MatType.CV_8UC4, new Scalar(255, 255, 255, 255));
            Cv2.ImEncode(".png", texture, out byte[] png);
            AddBytes(zip, "assets/minecraft/textures/item/spawn_egg.png", png);
        }

        /// <summary>
        /// zipに文字列のエントリを追加する。
        /// </summary>
        private static void AddText(ZipArchive zip, string entryName, string text)
        {
            AddBytes(zip, entryName, Encoding.UTF8.GetBytes(text));
        }

        /// <summary>
        /// zipにバイト列のエントリを追加する。
        /// </summary>
        private static void AddBytes(ZipArchive zip, string entryName, byte[] data)
        {
            using var stream = zip.CreateEntry(entryName).Open();
            stream.Write(data, 0, data.Length);
        }
    }
}
