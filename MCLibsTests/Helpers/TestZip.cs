using System.IO;
using System.IO.Compression;
using System.Text;
using OpenCvSharp;

namespace MCLibsTests.Helpers
{
    /// <summary>
    /// テスト用の zip（jar・リソースパック）を作成するヘルパー
    /// </summary>
    public static class TestZip
    {
        /// <summary>
        /// zip ファイルを作成する（親ディレクトリも作成する）。
        /// </summary>
        /// <param name="zipPath">作成する zip のパス</param>
        /// <param name="build">エントリを追加する処理</param>
        /// <returns>作成した zip のパス</returns>
        public static string Create(string zipPath, Action<ZipArchive> build)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(zipPath)!);
            using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
            build(zip);
            return zipPath;
        }

        /// <summary>
        /// 文字列のエントリを追加する。
        /// </summary>
        public static void AddText(this ZipArchive zip, string entryName, string text)
        {
            zip.AddBytes(entryName, Encoding.UTF8.GetBytes(text));
        }

        /// <summary>
        /// バイト列のエントリを追加する。
        /// </summary>
        public static void AddBytes(this ZipArchive zip, string entryName, byte[] data)
        {
            using var stream = zip.CreateEntry(entryName).Open();
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// ディレクトリのエントリ（末尾が "/" のエントリ）を追加する。
        /// </summary>
        public static void AddDirectory(this ZipArchive zip, string entryName)
        {
            zip.CreateEntry(entryName.EndsWith('/') ? entryName : entryName + "/");
        }

        /// <summary>
        /// 単色の PNG 画像のエントリを追加する。
        /// </summary>
        /// <param name="zip">追加先の zip</param>
        /// <param name="entryName">エントリ名</param>
        /// <param name="bgra">画素値（BGRA）</param>
        /// <param name="size">画像の一辺の長さ</param>
        public static void AddPng(this ZipArchive zip, string entryName, Scalar bgra, int size = 16)
        {
            zip.AddBytes(entryName, EncodePng(bgra, size));
        }

        /// <summary>
        /// 単色の PNG 画像をエンコードする。
        /// </summary>
        /// <param name="bgra">画素値（BGRA）</param>
        /// <param name="size">画像の一辺の長さ</param>
        /// <returns>PNG のバイト列</returns>
        public static byte[] EncodePng(Scalar bgra, int size = 16)
        {
            using var mat = new Mat(size, size, MatType.CV_8UC4, bgra);
            Cv2.ImEncode(".png", mat, out byte[] png);
            return png;
        }
    }
}
