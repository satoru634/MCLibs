using MCToolsCommonLib.Common;
using Nett;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using Windows.Globalization;

namespace MCToolsCommonLib.Utils
{
    /// <summary>
    /// JARファイルを操作するためのユーティリティクラス
    /// </summary>
    public class JarLoader
    {
        /// <summary>
        /// JARファイルのパスを格納するプロパティ
        /// </summary>
        protected string JarPath { get; set; }

        /// <summary>
        /// 言語設定を保持するプロパティ
        /// </summary>
        protected string LanguageCode { get; set; }

        /// <summary>
        /// バージョン情報を格納するプロパティ
        /// </summary>
        protected string Version { get; set; }

        /// <summary>
        /// 属性ファイルのパスを格納するプロパティ
        /// </summary>
        protected string AttrPath { get; set; }

        /// <summary>
        /// 名前空間を格納するプロパティ
        /// </summary>
        protected string NameSpace { get; set; }

        /// <summary>
        /// 指定されたJARファイルのパスを使用して新しいインスタンスを初期化する。
        /// </summary>
        /// <param name="jarPath">JARファイルのパス</param>
        public JarLoader(string jarPath, string languageCode, string version = "")
        {
            JarPath = jarPath;
            LanguageCode = languageCode;
            Version = version;
            AttrPath = Path.Combine(Path.GetDirectoryName(jarPath) ?? "", Path.GetFileNameWithoutExtension(jarPath)) + ".json";
            NameSpace = GetNameSpace();
        }

        /// <summary>
        /// デフォルトの値を使用して新しいインスタンスを初期化する。
        /// </summary>
        public JarLoader()
        {
            JarPath = "";
            LanguageCode = "";
            Version = "";
            AttrPath = "";
            NameSpace = "";
        }

        /// <summary>
        /// mincraftのパスを取得する。
        /// </summary>
        /// <returns></returns>
        public string GetMincraftPath()
        {
            return CommonLib.GetAncestorDirectory(JarPath, 3);
        }

        /// <summary>
        /// 指定されたパスがJARファイル内に存在するかどうかを確認する。
        /// </summary>
        /// <param name="targetPath"></param>
        /// <returns>存在する場合はtrue、存在しない場合はfalse</returns>
        public bool Exists(string targetPath)
        {
            using (ZipArchive zip = ZipFile.OpenRead(JarPath))
            {
                var found = zip.Entries.Where(entry => entry.FullName == targetPath).ToList();
                if (found.Count == 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// JARファイル内の名前空間を取得する。
        /// </summary>
        /// <returns>名前空間の文字列</returns>
        public string GetNameSpace()
        {
            string nameSpace = "minecraft";
            using (ZipArchive zip = ZipFile.OpenRead(JarPath))
            {
                var found = zip.Entries.Where(entry => entry.FullName == "META-INF/mods.toml").ToList();
                if (found.Count == 0)
                {
                    return nameSpace;
                }

                var toml = Toml.ReadStream(found[0].Open());
                var mods = toml.Get<TomlTableArray>("mods");
                nameSpace = mods[0].Get<string>("modId");
            }

            return nameSpace;
        }

        /// <summary>
        /// 指定されたパスのJSONデータを読み込み、指定された型にデシリアライズする。
        /// </summary>
        /// <typeparam name="T">デシリアライズする型</typeparam>
        /// <param name="targetPath">JSONデータのパス</param>
        /// <returns>デシリアライズされたデータ</returns>
        public T JsonLoad<T>(string targetPath) where T : new()
        {
            T data = new T();
            using (ZipArchive zip = ZipFile.OpenRead(JarPath))
            {
                var found = zip.Entries.Where(entry => entry.FullName == targetPath).ToList();
                if (found.Count == 0)
                {
                    return data;
                }

                data = CommonLib.ReadJson<T>(found[0].Open());
            }

            return data;
        }

        /// <summary>
        /// 指定されたパスのファイルリストを取得する。
        /// </summary>
        /// <param name="basePath">基準パス</param>
        /// <returns>ファイルリスト</returns>
        public List<string> GetFileList(string basePath)
        {
            var filList = new List<string>();
            using (ZipArchive zip = ZipFile.OpenRead(JarPath))
            {
                var entries = zip.Entries.Where(entry => entry.FullName.StartsWith(basePath)).ToList();
                foreach (var entry in entries)
                {
                    // basePathを除去して相対パスを取得
                    string relativePath = entry.FullName.Substring(basePath.Length).TrimStart('/');
                    filList.Add(relativePath);
                }
            }

            return filList;
        }

        /// <summary>
        /// アセットリストのパスを取得する。
        /// </summary>
        /// <returns></returns>
        public string GetAssetListPath()
        {
            var attr = CommonLib.ReadJson<Dictionary<string, object>>(AttrPath);
            var assetIndex = CommonLib.DeserializeJson<Dictionary<string, object>>(attr["assetIndex"].ToString());
            string assetListPath = Path.Combine(GetMincraftPath(), "assets", "indexes", assetIndex["id"].ToString() + ".json");
            return assetListPath;
        }

        /// <summary>
        /// 指定されたパスの画像データを読み込み、OpenCVのMat形式で返す。
        /// </summary>
        /// <param name="targetPath">画像データのパス</param>
        /// <returns>画像データを格納したMatオブジェクト</returns>
        public Mat<Vec4b> LoadImage(string targetPath)
        {
            Mat<Vec4b> img = new Mat<Vec4b>();
            using (ZipArchive zip = ZipFile.OpenRead(JarPath))
            {
                var found = zip.Entries.Where(entry => entry.FullName == targetPath).ToList();
                if (found.Count == 0)
                {
                    return img;
                }

                using (Stream entryStream = found[0].Open())
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    entryStream.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // 画像データをOpenCVのMat形式に変換
                    using var mat = Mat.FromImageData(memoryStream.ToArray(), ImreadModes.Unchanged);
                    if (mat.Type() == MatType.CV_8UC3)
                    {
                        // 3チャンネル(BGR)→4チャンネル(BGRA)へ変換
                        using var mat4 = new Mat();
                        Cv2.CvtColor(mat, mat4, ColorConversionCodes.BGR2BGRA);
                        img = new Mat<Vec4b>(mat4);
                    }
                    else
                    {
                        // 4チャンネル(BGRA)のまま
                        img = new Mat<Vec4b>(mat);
                    }
                }
            }

            return img;
        }

        /// <summary>
        /// リソースのハッシュ値を取得する。
        /// </summary>
        /// <returns>リソースのハッシュ値</returns>
        public string GetResourceHash()
        {
            var assets = CommonLib.ReadJson<Dictionary<string, object>>(GetAssetListPath());
            var assetObjects = CommonLib.DeserializeJson<Dictionary<string, object>>(assets["objects"].ToString());

            // _languageと部分一致するキーを抽出し、リストに格納
            var filteredKeys = assetObjects.Keys
                .Where(key => key.Contains(LanguageCode, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // フィルタリングされたキーが存在しない場合は何もしない
            if (filteredKeys.Count == 0)
            {
                return "";
            }

            // 最初のフィルタリングされたキーを使用して、リソースのハッシュを取得
            var assetObject = CommonLib.DeserializeJson<Dictionary<string, object>>(assetObjects[filteredKeys[0]].ToString());
            return assetObject["hash"].ToString() ?? "";
        }

        /// <summary>
        /// リソースのパスを生成する。
        /// </summary>
        /// <returns>リソースのパス</returns>
        public string GenerateResourcePath()
        {
            string resourcePath = Path.Combine(GetMincraftPath(), "assets", "objects", GetResourceHash().Substring(0, 2), GetResourceHash());
            return resourcePath;
        }
    }
}
