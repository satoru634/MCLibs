using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.Util
{
    public class ResourcePackInfoData
    {
        public string ResourcePackName { get; set; } = "";
        public string Type { get; set; } = "";
        public string NameSpace { get; set; } = "";
        public string AssetsDirPath { get; set; } = "";
    }

    /// <summary>
    /// リソースパックの情報を管理するクラス
    /// </summary>
    public class ResourcePackInfo
    {
        /// <summary>
        /// リソースパックの情報を格納するリスト
        /// </summary>
        public List<ResourcePackInfoData> Data { get; set; }

        /// <summary>
        /// リソースパックのベースパス
        /// </summary>
        private string _resourcePackBasePath;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="resourcePackBasePath"></param>
        public ResourcePackInfo(string resourcePackBasePath)
        {
            Data = new List<ResourcePackInfoData>();
            _resourcePackBasePath = resourcePackBasePath;
            return;
        }

        /// <summary>
        /// リソースパックの情報を取得
        /// </summary>
        public void GetList()
        {
            Data.AddRange(GetDirectoryList());
            Data.AddRange(GetRootFileForZip());
            return;
        }

        /// <summary>
        /// 指定された名前空間とパスに基づいてリソースパックを検索
        /// </summary>
        /// <param name="nameSpace">名前空間</param>
        /// <param name="targetPath">リソースパック内のパス</param>
        /// <returns>リソースパック内のパスが存在すれば、そのパスとタイプを返す</returns>
        public (string path, string type) SearchResource(string nameSpace, string targetPath)
        {
            string foundPath = "";
            string foundType = "";
            List<ResourcePackInfoData> filter = Data.Where(d => d.NameSpace == nameSpace).ToList();

            foreach (ResourcePackInfoData info in filter)
            {
                switch (info.Type)
                {
                    // ディレクトリの場合
                    case "dir":
                        string path = Path.Combine(info.AssetsDirPath, targetPath.Replace("/", "\\"));
                        if (Path.Exists(path))
                        {
                            // パスが存在する場合はそのパスを返す
                            foundPath = path;
                            foundType = info.Type;
                        }

                        break;

                    // ZIPファイルの場合
                    case "zip":
                        targetPath = $"assets/{targetPath}";
                        using (ZipArchive zip = ZipFile.OpenRead(info.AssetsDirPath))
                        {
                            var found = zip.Entries.Where(entry => entry.FullName == targetPath).ToList();
                            if (found.Count > 0)
                            {
                                // ZIP内でパスが見つかった場合はそのパスを返す
                                foundPath = info.AssetsDirPath;
                                foundType = info.Type;
                            }
                        }

                        break;
                }

                // 見つかったパスが空でない場合はループを抜ける
                if (foundPath != "")
                {
                    break;
                }
            }

            return (foundPath, foundType);
        }

        /// <summary>
        /// 指定された名前空間とパスに基づいてリソースが存在するかどうかを確認
        /// </summary>
        /// <param name="nameSpace">名前空間</param>
        /// <param name="targetPath">リソースパック内のパス</param>
        /// <returns>リソースパック内のパスが存在すればtrue、それ以外はfalse</returns>
        public bool ExistsResource(string nameSpace, string targetPath)
        {
            (string path, string type) = SearchResource(nameSpace, targetPath);
            return !string.IsNullOrEmpty(path);
        }

        /// <summary>
        /// テクスチャデータ取得
        /// </summary>
        /// <param name="tracker">リソーストラッカー</param>
        /// <param name="path">リソースパック内のパス</param>
        /// <param name="nameSpace">名前空間</param>
        /// <returns>リソースパックから取得した画像</returns>
        public Mat<Vec4b> GetTextureData(in ResourcesTracker tracker, string path, string nameSpace)
        {
            Mat<Vec4b> img;
            (string resourcePath, string type) = SearchResource(nameSpace, path);
            if (type == "dir")
            {
                // リソースパックのディレクトリから画像を取得
                img = tracker.T(new Mat<Vec4b>(Cv2.ImRead(resourcePath, ImreadModes.Unchanged)));
            }
            else
            {
                // リソースパックのZIPから画像を取得
                img = GetTextureFromZip(tracker, resourcePath, $"assets/{path}");
            }

            return img;
        }

        /// <summary>
        /// リソースパック(zip)から画像を取得
        /// </summary>
        /// <param name="tracker">リソーストラッカー</param>
        /// <param name="resourcePath">リソースパックのパス</param>
        /// <param name="targetPath">対象のリソースパス</param>
        /// <returns>リソースパックから取得した画像</returns>
        public Mat<Vec4b> GetTextureFromZip(in ResourcesTracker tracker, string resourcePath, string targetPath)
        {
            Mat<Vec4b> img = tracker.T(new Mat<Vec4b>());
            using (ZipArchive zip = ZipFile.OpenRead(resourcePath))
            {
                var found = zip.Entries.Where(entry => entry.FullName == targetPath).ToList();

                using (Stream entryStream = found[0].Open())
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    entryStream.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    // 画像データをOpenCVのMat形式に変換
                    img = tracker.T(new Mat<Vec4b>(Mat.FromImageData(memoryStream.ToArray(), ImreadModes.Unchanged)));
                }
            }

            return img;
        }

        /// <summary>
        /// ZIPファイルタイプのリソースパック情報を取得
        /// </summary>
        /// <returns>リソースパック情報のリスト</returns>
        private List<ResourcePackInfoData> GetRootFileForZip()
        {
            List<ResourcePackInfoData> infoList = new List<ResourcePackInfoData>();
            foreach (string zipFilePath in Directory.EnumerateFiles(_resourcePackBasePath, "*.zip", SearchOption.TopDirectoryOnly).ToList())
            {
                string zipFileName = Path.GetFileName(zipFilePath);

                using (ZipArchive zip = ZipFile.OpenRead(zipFilePath))
                {
                    foreach (var filter in zip.Entries.Where(entry => entry.FullName.Split('/').Length == 3).ToList())
                    {
                        ResourcePackInfoData info = new ResourcePackInfoData();
                        info.ResourcePackName = zipFileName;
                        info.Type = "zip";
                        info.NameSpace = filter.FullName.Replace("assets/", "").Split('/')[0];
                        info.AssetsDirPath = zipFilePath;
                        infoList.Add(info);
                    }
                }

            }

            return infoList;
        }

        /// <summary>
        /// ディレクトリタイプのリソースパック情報を取得
        /// </summary>
        /// <returns>リソースパック情報のリスト</returns>
        private List<ResourcePackInfoData> GetDirectoryList()
        {
            List<ResourcePackInfoData> infoList = new List<ResourcePackInfoData>();
            foreach (string dir in Directory.EnumerateDirectories(_resourcePackBasePath, "*assets", SearchOption.AllDirectories).ToList())
            {
                string? resourceName = Path.GetFileName(Path.GetDirectoryName(dir));
                foreach (string root in Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly).ToList())
                {
                    string? dirName = Path.GetDirectoryName(root);
                    ResourcePackInfoData info = new ResourcePackInfoData();
                    info.ResourcePackName = resourceName != null ? resourceName : "none";
                    info.Type = "dir";
                    info.NameSpace = Path.GetFileName(root);
                    info.AssetsDirPath = dirName != null ? dirName : "";
                    infoList.Add(info);
                }
            }

            return infoList;
        }
    }
}
