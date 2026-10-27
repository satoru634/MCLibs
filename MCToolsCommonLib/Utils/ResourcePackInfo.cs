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

    public class ResourcePackInfo
    {
        public List<ResourcePackInfoData> Data { get; set; }

        private string ResourcePackBasePath { get; set; }

        public ResourcePackInfo(string resourcePackBasePath)
        {
            Data = new List<ResourcePackInfoData>();
            ResourcePackBasePath = resourcePackBasePath;
            return;
        }

        public void GetList()
        {
            Data.AddRange(GetDirectoryList());
            Data.AddRange(GetRootFileForZip());
            return;
        }

        public (string path, string type) SearchResource(string nameSpace, string targetPath)
        {
            string foundPath = "";
            string foundType = "";
            List<ResourcePackInfoData> filter = Data.Where(d => d.NameSpace == nameSpace).ToList();

            foreach (ResourcePackInfoData info in filter)
            {
                switch (info.Type)
                {
                    case "dir":
                        string path = Path.Combine(info.AssetsDirPath, targetPath);
                        if (Path.Exists(path))
                        {
                            foundPath = path;
                            foundType = info.Type;
                        }

                        break;

                    case "zip":
                        targetPath = $"assets/{targetPath.Replace("\\", "/")}";
                        using (ZipArchive zip = ZipFile.OpenRead(info.AssetsDirPath))
                        {
                            var found = zip.Entries.Where(entry => entry.FullName == targetPath).ToList();
                            if (found.Count > 0)
                            {
                                foundPath = info.AssetsDirPath;
                                foundType = info.Type;
                            }
                        }

                        break;
                }

                if (foundPath != "")
                {
                    break;
                }
            }

            return (foundPath, foundType);
        }

        private List<ResourcePackInfoData> GetRootFileForZip()
        {
            List<ResourcePackInfoData> infoList = new List<ResourcePackInfoData>();
            foreach (string zipFilePath in Directory.EnumerateFiles(ResourcePackBasePath, "*.zip", SearchOption.TopDirectoryOnly).ToList())
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

        private List<ResourcePackInfoData> GetDirectoryList()
        {
            List<ResourcePackInfoData> infoList = new List<ResourcePackInfoData>();
            foreach (string dir in Directory.EnumerateDirectories(ResourcePackBasePath, "*assets", SearchOption.AllDirectories).ToList())
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
