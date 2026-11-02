using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace MCLibsTests.Helpers
{
    /// <summary>
    /// テスト用の .minecraft ディレクトリ構成を作成するヘルパー
    /// </summary>
    public static class TestMinecraft
    {
        /// <summary>
        /// versions/&lt;バージョン&gt;/&lt;バージョン&gt;.jar を作成する。
        /// </summary>
        /// <param name="minecraftPath">.minecraft とみなすディレクトリ</param>
        /// <param name="version">バージョン</param>
        /// <param name="build">jar にエントリを追加する処理</param>
        /// <returns>作成した jar のパス</returns>
        public static string CreateJar(string minecraftPath, string version, Action<ZipArchive> build)
        {
            return TestZip.Create(Path.Combine(minecraftPath, "versions", version, $"{version}.jar"), build);
        }

        /// <summary>
        /// バージョン情報（versions/&lt;バージョン&gt;/&lt;バージョン&gt;.json）とアセットインデックス
        /// （assets/indexes/&lt;ID&gt;.json）を作成する。
        /// </summary>
        /// <param name="minecraftPath">.minecraft とみなすディレクトリ</param>
        /// <param name="version">バージョン</param>
        /// <param name="indexId">アセットインデックスの ID</param>
        /// <param name="objects">アセットのキー（例: minecraft/lang/ja_jp.json）とハッシュの組</param>
        public static void CreateAssetIndex(string minecraftPath, string version, string indexId, Dictionary<string, string> objects)
        {
            string versionDir = Path.Combine(minecraftPath, "versions", version);
            Directory.CreateDirectory(versionDir);
            File.WriteAllText(Path.Combine(versionDir, $"{version}.json"),
                JsonSerializer.Serialize(new { assetIndex = new { id = indexId } }));

            string indexDir = Path.Combine(minecraftPath, "assets", "indexes");
            Directory.CreateDirectory(indexDir);
            var assetObjects = objects.ToDictionary(o => o.Key, o => new { hash = o.Value, size = 0 });
            File.WriteAllText(Path.Combine(indexDir, $"{indexId}.json"),
                JsonSerializer.Serialize(new { objects = assetObjects }));
        }

        /// <summary>
        /// アセットの実体（assets/objects/&lt;ハッシュ先頭2文字&gt;/&lt;ハッシュ&gt;）を作成する。
        /// </summary>
        /// <param name="minecraftPath">.minecraft とみなすディレクトリ</param>
        /// <param name="hash">ハッシュ</param>
        /// <param name="content">ファイルの内容</param>
        /// <returns>作成したファイルのパス</returns>
        public static string CreateAssetObject(string minecraftPath, string hash, string content)
        {
            string dir = Path.Combine(minecraftPath, "assets", "objects", hash.Substring(0, 2));
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, hash);
            File.WriteAllText(path, content);
            return path;
        }
    }
}
