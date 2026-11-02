using System.IO;

namespace MCLibsTests.Helpers
{
    /// <summary>
    /// テスト用の一時ディレクトリ。Dispose 時に配下ごと削除する。
    /// </summary>
    public sealed class TempDirectory : IDisposable
    {
        /// <summary>
        /// 一時ディレクトリのパス
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// 一時ディレクトリを作成する。
        /// </summary>
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MCLibsTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        /// <summary>
        /// 一時ディレクトリ配下のパスを生成する（ディレクトリは作成しない）。
        /// </summary>
        /// <param name="relativePaths">一時ディレクトリからの相対パス</param>
        /// <returns>結合したパス</returns>
        public string Combine(params string[] relativePaths)
        {
            return System.IO.Path.Combine([Path, .. relativePaths]);
        }

        /// <summary>
        /// 一時ディレクトリ配下にディレクトリを作成する。
        /// </summary>
        /// <param name="relativePaths">一時ディレクトリからの相対パス</param>
        /// <returns>作成したディレクトリのパス</returns>
        public string CreateDirectory(params string[] relativePaths)
        {
            string path = Combine(relativePaths);
            Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// 一時ディレクトリ配下にテキストファイルを作成する（親ディレクトリも作成する）。
        /// </summary>
        /// <param name="text">書き込む文字列</param>
        /// <param name="relativePaths">一時ディレクトリからの相対パス</param>
        /// <returns>作成したファイルのパス</returns>
        public string WriteText(string text, params string[] relativePaths)
        {
            string path = Combine(relativePaths);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text);
            return path;
        }

        /// <summary>
        /// 一時ディレクトリを削除する。
        /// </summary>
        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
