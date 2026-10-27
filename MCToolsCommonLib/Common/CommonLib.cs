using CsvHelper;
using CsvHelper.Configuration;
using fNbt;
using MCToolsCommonLib.BaseData;
using OpenCvSharp;
using System.Diagnostics;
using System.Drawing;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows.Media.Media3D;
using Point = OpenCvSharp.Point;

namespace MCToolsCommonLib.Common
{
    public class CommonLib
    {
        /// <summary>
        /// UUIDを整数配列から文字列に変換する。
        /// </summary>
        /// <param name="intUUID">UUID (int配列)</param>
        /// <returns>UUID(文字列)</returns>
        static public string ConvertUUID(int[] intUUID)
        {
            // UUIDは4つのintで構成されているため、長さが4でない場合は空文字を返す
            if (intUUID.Length != 4)
            {
                return "";
            }

            // UUIDを16進数の文字列に変換
            string orgUUID = "";
            foreach (int id in intUUID)
            {
                orgUUID += (id & 0xFFFFFFFF).ToString("x8");
            }

            // UUIDをUUID形式の文字列に変換
            string uuid = $"{orgUUID.Substring(0, 8)}-{orgUUID.Substring(8, 4)}-{orgUUID.Substring(12, 4)}-{orgUUID.Substring(16, 4)}-{orgUUID.Substring(20, 12)}";
            return uuid;
        }

        /// <summary>
        /// 2つの3次元座標の中心を取得する。
        /// </summary>
        /// <param name="start">開始座標</param>
        /// <param name="end">終了座標</param>
        /// <returns>中心座標</returns>
        static public BaseData.Point3D GetBoxCenter(BaseData.Point3D start, BaseData.Point3D end)
        {
            BaseData.Point3D center = new BaseData.Point3D();
            center.X = (end.X - start.X) / 2 + start.X;
            center.Y = (end.Y - start.Y) / 2 + start.Y;
            center.Z = (end.Z - start.Z) / 2 + start.Z;
            return center;
        }

        /// <summary>
        /// 2点間の距離を算出する。
        /// </summary>
        /// <param name="basePoint">基準座標</param>
        /// <param name="targetPoint">目標座標</param>
        /// <returns>2点間距離</returns>
        static public double CalcDistance2D(BaseData.Point3D basePoint, BaseData.Point3D targetPoint)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(basePoint.X - targetPoint.X), 2) +
                Math.Pow(Math.Abs(basePoint.Z - targetPoint.Z), 2));
        }

        /// <summary>
        /// 2点間の距離を算出する。
        /// </summary>
        /// <param name="basePoint">基準座標</param>
        /// <param name="targetPoint">目標座標</param>
        /// <returns>2点間距離</returns>
        static public double CalcDistance2D(PointXZ basePoint, PointXZ targetPoint)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(basePoint.X - targetPoint.X), 2) +
                Math.Pow(Math.Abs(basePoint.Z - targetPoint.Z), 2));
        }

        /// <summary>
        /// 2点間の距離を算出する。
        /// </summary>
        /// <param name="basePoint">基準座標</param>
        /// <param name="targetPoint">目標座標</param>
        /// <returns>2点間距離</returns>
        static public double CalcDistance2D(BaseData.Point3D basePoint, PointXZ targetPoint)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(basePoint.X - targetPoint.X), 2) +
                Math.Pow(Math.Abs(basePoint.Z - targetPoint.Z), 2));
        }

        /// <summary>
        /// 2点間の距離を算出する。
        /// </summary>
        /// <param name="basePoint">基準座標</param>
        /// <param name="targetPoint">目標座標</param>
        /// <returns>2点間距離</returns>
        static public double CalcDistance2D(PointXZ basePoint, BaseData.Point3D targetPoint)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(basePoint.X - targetPoint.X), 2) +
                Math.Pow(Math.Abs(basePoint.Z - targetPoint.Z), 2));
        }

        /// <summary>
        /// ワールドフォルダ一覧を取得する。
        /// </summary>
        /// <param name="path">Minecraftパス</param>
        /// <returns>ワールドフォルダ一覧</returns>
        static public Dictionary<string, string> GetWorldFolderList(string path)
        {
            Dictionary<string, string> worldList = new Dictionary<string, string>();
            foreach (string target in Directory.EnumerateDirectories(Path.Combine(path, "saves"), "*", SearchOption.TopDirectoryOnly).ToList())
            {
                if (!Path.Exists(Path.Combine(target, "level.dat")))
                {
                    continue;
                }

                worldList.Add(Path.GetFileName(target), target);
            }

            return worldList;
        }

        /// <summary>
        /// ワールドのディメンション一覧を取得する。
        /// </summary>
        /// <param name="path">Minecraftパス</param>
        /// <param name="worldName">ワールド名</param>
        /// <returns>ディメンション一覧</returns>
        static public Dictionary<string, string> GetDimensionList(string path, string worldName)
        {
            Dictionary<string, string> dimensionList = new Dictionary<string, string>();
            foreach (string target in Directory.EnumerateDirectories(path, "entities", SearchOption.AllDirectories).ToList())
            {
                string dimensionName = ConvertDimensionName(Path.GetFileName(Path.GetDirectoryName(target) ?? ""), worldName);
                dimensionList.Add(dimensionName, target);
            }

            return dimensionList;
        }

        /// <summary>
        /// ディメンションフォルダ名を本来のディメンション名に変換する。
        /// </summary>
        /// <param name="dimensionName">元のディメンション名</param>
        /// <param name="worldName">ワールド名</param>
        /// <returns>ディメンション名</returns>
        static public string ConvertDimensionName(string dimensionName, string worldName)
        {
            string newDimensionName = "";
            if (dimensionName == worldName)
            {
                newDimensionName = "overworld";
            }
            else if (dimensionName == "DIM-1")
            {
                newDimensionName = "nether";
            }
            else if (dimensionName == "DIM1")
            {
                newDimensionName = "end";
            }
            else
            {
                newDimensionName = dimensionName;
            }

            return newDimensionName;
        }

        /// <summary>
        /// Minecraftのプログラムファイル一覧を取得する。
        /// </summary>
        /// <param name="minecraftPath">Minecraftパス</param>
        /// <param name="limitVersion">制限するバージョン</param>
        /// <returns>プログラムファイル一覧</returns>
        static public List<string> GetMinecraftProgramFiles(string minecraftPath, string limitVersion = "")
        {
            List<string> programList = new List<string>();

            string versionPath = Path.Combine(minecraftPath, "versions");
            if (!Directory.Exists(versionPath))
            {
                // バージョンディレクトリが存在しない場合は空のリストを返す
                return programList;
            }

            foreach (string programPath in Directory.EnumerateFiles(versionPath, "*.jar", SearchOption.AllDirectories).ToList())
            {
                string version = Path.GetFileNameWithoutExtension(programPath);
                if ((limitVersion != "") && (CompareVersion(version, limitVersion) < 0))
                {
                    // 制限バージョンより古い場合はスキップ
                    continue;
                }

                programList.Add(version);
            }

            return programList;
        }

        /// <summary>
        /// 指定されたMinecraftパスとバージョンに基づいて、Minecraftのプログラムファイルのパスを取得する。
        /// </summary>
        /// <param name="minecraftPath">Minecraftパス</param>
        /// <param name="version">Minecraftバージョン</param>
        /// <returns>プログラムファイルのパス</returns>
        static public string GetMinecraftProgramFilePath(string minecraftPath, string version)
        {
            string targetProgramPath = "";

            string versionPath = Path.Combine(minecraftPath, "versions");
            if (!Directory.Exists(versionPath))
            {
                // バージョンディレクトリが存在しない場合は空文字を返す
                return targetProgramPath;
            }

            foreach (string programPath in Directory.EnumerateFiles(versionPath, "*.jar", SearchOption.AllDirectories).ToList())
            {
                if (Path.GetFileNameWithoutExtension(programPath) == version)
                {
                    targetProgramPath = programPath;
                }
            }

            return targetProgramPath;
        }

        /// <summary>
        /// 指定されたlevel.datからプレイヤーのUUIDを取得する。
        /// </summary>
        /// <param name="lavelPath">level.datのパス</param>
        /// <returns>プレイヤーのUUID</returns>
        static public string GetPlayerUUID(string lavelPath)
        {
            NbtFile nbt = new NbtFile();
            nbt.LoadFromFile(lavelPath);
            return ConvertUUID(nbt.RootTag["Data"]["Player"]["UUID"].IntArrayValue);
        }

        /// <summary>
        /// 0 ～ 最大値の範囲でクランプする。
        /// </summary>
        /// <param name="val">値</param>
        /// <param name="maxVal">最大値</param>
        /// <returns>クランプされた値</returns>
        static public T ClampFloor<T>(T val, T maxVal) where T : INumber<T>
        {
            return Math.Min((dynamic)maxVal, Math.Max((dynamic)val * (dynamic)maxVal, 0));
        }

        /// <summary>
        /// データタグを生成する(List)。
        /// </summary>
        /// <param name="tagList">リスト(NBT)</param>
        /// <returns>データタグ</returns>
        static public string GenerateDataTagList(NbtList tagList)
        {
            List<string> tagStrList = new List<string>();
            foreach (NbtTag tag in tagList)
            {
                tagStrList.Add(GenerateDataTagCompound((NbtCompound)tag));
            }

            return $"[{string.Join(",", tagStrList)}]";
        }

        /// <summary>
        /// 2つのバージョン文字列を比較する。
        /// </summary>
        /// <param name="versionA">比較元バージョン</param>
        /// <param name="versionB">比較先バージョン</param>
        /// <returns>
        /// versionAがversionBより大きい場合は1、
        /// versionAがversionBより小さい場合は-1、
        /// 同じ場合は0
        /// </returns>
        static public int CompareVersion(string versionA, string versionB)
        {
            // バージョンが空文字またはnullの場合の処理
            if (string.IsNullOrWhiteSpace(versionA) && string.IsNullOrWhiteSpace(versionB))
            {
                return 0;
            }
            else if (string.IsNullOrWhiteSpace(versionA))
            {
                return -1;
            }
            else if (string.IsNullOrWhiteSpace(versionB))
            {
                return 1;
            }

            // バージョン文字列をドットで分割し、各部分を整数に変換して比較する
            string[] partsA = versionA.Split('.');
            string[] partsB = versionB.Split('.');
            int maxLen = Math.Max(partsA.Length, partsB.Length);

            // 各部分を整数に変換して比較
            for (int i = 0; i < maxLen; i++)
            {
                int a = i < partsA.Length && int.TryParse(partsA[i], out int valA) ? valA : 0;
                int b = i < partsB.Length && int.TryParse(partsB[i], out int valB) ? valB : 0;
                if (a > b)
                {
                    return 1;
                }
                else if (a < b)
                {
                    return -1;
                }
            }

            return 0;
        }

        /// <summary>
        /// 2つの値が等しいかどうかを比較する。
        /// </summary>
        /// <param name="a">値A</param>
        /// <param name="b">値B</param>
        /// <returns>等しい場合はtrue、そうでない場合はfalse</returns>
        static public bool IsEqual(float a, float b)
        {
            if (Math.Abs(a - b) <= 1e-6F)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 2つの値が等しいかどうかを比較する。
        /// </summary>
        /// <param name="a">値A</param>
        /// <param name="b">値B</param>
        /// <returns>等しい場合はtrue、そうでない場合はfalse</returns>
        static public bool IsEqual(double a, double b)
        {
            if (Math.Abs(a - b) <= 1e-9F)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// データタグを生成する(Compound)。
        /// </summary>
        /// <param name="tagCompound">Compound(NBT)</param>
        /// <returns>データタグ</returns>
        static public string GenerateDataTagCompound(NbtCompound tagCompound)
        {
            List<string> tagStrList = new List<string>();
            foreach (NbtTag tag in tagCompound)
            {
                string tagStr = $"{tag.Name}:";
                switch (tag.TagType)
                {
                    case NbtTagType.Compound:
                        tagStr += GenerateDataTagCompound((NbtCompound)tag);
                        break;

                    case NbtTagType.List:
                        tagStr += GenerateDataTagList((NbtList)tag);
                        break;

                    case NbtTagType.Int:
                        tagStr += tag.IntValue.ToString();
                        break;

                    case NbtTagType.String:
                        tagStr += tag.StringValue;
                        break;
                }

                tagStrList.Add(tagStr);
            }

            return $"{{{string.Join(",", tagStrList)}}}";
        }

        /// <summary>
        /// アイテム情報を取得する。
        /// </summary>
        /// <param name="item">NBTタグ</param>
        /// <returns>アイテム情報(アイテム名, アイテム数)</returns>
        static public (string itemName, int itemCount) GetItemInfo(NbtTag item)
        {
            string id = item["id"].StringValue;
            int count = item["Count"].IntValue;
            string tag = item["tag"] != null ? GenerateDataTagCompound((NbtCompound)item["tag"]) : "";
            return ($"{id}{tag}", count);
        }

        /// <summary>
        /// 指定されたパスからN階層上のディレクトリを取得する。
        /// </summary>
        /// <param name="path">基準パス</param>
        /// <param name="level">階層</param>
        /// <returns></returns>
        static public string GetAncestorDirectory(string path, int level)
        {
            string targetPath = path;

            // ループで指定階層まで遡る
            for (int i = 0; i < level; i++)
            {
                targetPath = Path.GetDirectoryName(targetPath) ?? "";
                if (targetPath == "")
                {
                    break;
                }
            }

            return targetPath;
        }

        /// <summary>
        /// 指定されたパスからCSVデータを読み込み、指定された型にデシリアライズする。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TMap"></typeparam>
        /// <param name="inputPath">入力パス</param>
        /// <returns></returns>
        static public List<T> ReadCsv<T, TMap>(string inputPath) where TMap : ClassMap<T>
        {
            List<T> data = new List<T>();
            using (StreamReader sr = new StreamReader(inputPath))
            using (CsvReader csv = new CsvReader(sr, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<TMap>();
                data = csv.GetRecord<List<T>>();
            }

            return data;
        }

        /// <summary>
        /// 指定されたデータをCSV形式でファイルに書き込む。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TMap"></typeparam>
        /// <param name="outputPath">出力先パス</param>
        /// <param name="data">CSVに書き込むデータ</param>
        static public void WriteCsv<T, TMap>(string outputPath, in List<T> data) where TMap : ClassMap<T>
        {
            using (StreamWriter sw = new StreamWriter(outputPath))
            using (CsvWriter csv = new CsvWriter(sw, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<TMap>();
                csv.WriteRecords(data);
            }

            return;
        }

        /// <summary>
        /// 指定されたパスからJSONデータを読み込み、指定された型にデシリアライズする。
        /// </summary>
        /// <param name="inputPath">入力パス</param>
        /// <returns></returns>
        static public T ReadJson<T>(string inputPath) where T : new()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(inputPath))
            {
                json = sr.ReadToEnd();
            }

            return DeserializeJson<T>(json);
        }

        /// <summary>
        /// 指定されたストリームからJSONデータを読み込み、指定された型にデシリアライズする。
        /// </summary>
        /// <param name="inputPath">入力パス</param>
        /// <returns></returns>
        static public T ReadJson<T>(Stream inputStream) where T : new()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(inputStream))
            {
                json = sr.ReadToEnd();
            }

            return DeserializeJson<T>(json);
        }

        /// <summary>
        /// 指定されたJSON文字列をデシリアライズして、指定された型のオブジェクトを返す。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        static public T DeserializeJson<T>(string? json) where T : new()
        {
            if (json == null)
            {
                return new T();
            }

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            options.WriteIndented = true;
            T? data = JsonSerializer.Deserialize<T>(json, options);
            return data != null ? data : new T();
        }

        /// <summary>
        /// 指定されたデータをJSON形式でファイルに書き込む。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="outputPath">出力先パス</param>
        /// <param name="data">書き込み対象のデータ</param>
        static public void WriteJson<T>(string outputPath, in T data)
        {
            // JSONのオプションを設定
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            options.WriteIndented = true;

            // ディレクトリが存在しない場合は作成
            if (!Path.Exists(Path.GetDirectoryName(outputPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "");
            }

            // JSONをシリアライズしてファイルに書き込む
            string json = JsonSerializer.Serialize(data, options);
            using (StreamWriter sw = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                sw.WriteLine(json);
            }

            return;
        }
    }
}
