using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using fNbt;
using CsvHelper;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using CsvHelper.Configuration;
using OpenCvSharp;
using System.Windows.Media.Imaging;
using MCToolsCommonLib.BaseData;
using MCToolsCommonLib.Common;
using MCToolsCommonLib.Utils;
using MCALib.Data;

namespace MCALib.Utils
{
    /// <summary>
    /// 村人情報を生成・管理するクラス
    /// </summary>
    public class VillagerInfoGenerator : IDisposable
    {
        /// <summary>
        /// 村人の情報リスト
        /// </summary>
        public List<MCAVillagerInfo> VillagerList { get; private set; }

        /// <summary>
        /// 村の基本情報リスト
        /// </summary>
        public List<VillageInfo> VillageList { get; private set; }

        /// <summary>
        /// 初期化済みフラグ
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// ゲームディレクトリのパス
        /// </summary>
        private string _gameDirectory;

        /// <summary>
        /// ワールド名
        /// </summary>
        private string _worldName;

        /// <summary>
        /// 出力先のベースパス
        /// </summary>
        private string _outputBasePath;

        /// <summary>
        /// プレイヤーのUUID
        /// </summary>
        private string _playerUUID;

        /// <summary>
        /// セーブデータのパス
        /// </summary>
        private string _savePath = "";

        /// <summary>
        /// リージョン結果のベースパス
        /// </summary>
        private string _regionResultBasePath = "";

        /// <summary>
        /// 住民リスト
        /// </summary>
        private Dictionary<string, string> _regidentList;

        /// <summary>
        /// 処理済みエンティティファイルリスト
        /// </summary>
        private Dictionary<string, string> _entitiesList;

        /// <summary>
        /// 村人のスキンを生成するためのインスタンス
        /// </summary>
        private VillagerSkinGenerator _skinGenerator;

        /// <summary>
        /// 処理キャンセル用のトークンソース
        /// </summary>
        private CancellationTokenSource? _cancelTokenSrc = null;

        /// <summary>
        /// 破棄済みフラグ
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public VillagerInfoGenerator()
        {
            VillagerList = new List<MCAVillagerInfo>();
            VillageList = new List<VillageInfo>();
            IsInitialized = false;
            _gameDirectory = "";
            _worldName = "";
            _outputBasePath = "";
            _playerUUID = "";
            _savePath = "";
            _regionResultBasePath = "";
            _regidentList = new Dictionary<string, string>();
            _entitiesList = new Dictionary<string, string>();
            _skinGenerator = new VillagerSkinGenerator();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="gameDirectory">ゲームディレクトリ</param>
        /// <param name="worldName">ワールド名</param>
        /// <param name="outputBasePath">出力先のベースパス</param>
        public VillagerInfoGenerator(string gameDirectory, string worldName, string outputBasePath)
        {
            VillagerList = new List<MCAVillagerInfo>();
            VillageList = new List<VillageInfo>();
            IsInitialized = false;

            _gameDirectory = gameDirectory;
            _worldName = worldName;
            _outputBasePath = Path.Combine(outputBasePath, worldName);

            _savePath = Path.Combine(_gameDirectory, "saves", _worldName);

            _regionResultBasePath = Path.Combine(_outputBasePath, "_region");
            _playerUUID = CommonLib.GetPlayerUUID(Path.Combine(_savePath, "level.dat"));

            _regidentList = new Dictionary<string, string>();
            _entitiesList = new Dictionary<string, string>();

            _skinGenerator = new VillagerSkinGenerator(_gameDirectory);

            if (!Directory.Exists(_outputBasePath))
            {
                Directory.CreateDirectory(_outputBasePath);
            }

            if (!Directory.Exists(_regionResultBasePath))
            {
                Directory.CreateDirectory(_regionResultBasePath);
            }

            IsInitialized = true;
            return;
        }

        /// <summary>
        /// Disposeメソッド
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed == false)
            {
                disposed = true;
                IsInitialized = false;
                VillageList.Clear();
                VillagerList.Clear();
                _regidentList.Clear();
                _entitiesList.Clear();
                _skinGenerator.Dispose();

                if (disposing == true)
                {
                    GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>
        /// Disposeメソッド
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~VillagerInfoGenerator()
        {
            Dispose(false);
        }

        /// <summary>
        /// エンティティファイルの一覧を取得する
        /// </summary>
        /// <returns>エンティティファイル一覧</returns>
        public List<string> GetEntitiesFileList()
        {
            string entityPath = Path.Combine(_savePath, "entities");
            return Directory.EnumerateFiles(entityPath, "*.mca", SearchOption.TopDirectoryOnly).ToList();
        }

        /// <summary>
        /// エンティティファイルの数を取得する
        /// </summary>
        /// <returns></returns>
        public int GetEntitiesFileCount()
        {
            return GetEntitiesFileList().Count;
        }

        /// <summary>
        /// 全てのエンティティ情報を生成するメソッド
        /// </summary>
        /// <param name="progress">進捗状況</param>
        /// <returns></returns>
        public async Task StartGenerateInfoAll(IProgress<ProgressInfo> progress)
        {
            // 村の基本情報の生成
            List<string> entityFileList = GetEntitiesFileList();
            InitData();

            // 別スレッドでエンティティ情報の生成を行う
            await Task.Run(() =>
            {
                GenerateInfoAll(entityFileList, progress);
            });

            return;
        }

        /// <summary>
        /// 全てのエンティティ情報を生成する
        /// </summary>
        /// <param name="entityFileList">エンティティファイルリスト</param>
        /// <param name="progress">進捗状況</param>
        private void GenerateInfoAll(List<string> entityFileList, IProgress<ProgressInfo> progress)
        {
            int count = 0;
            using (_cancelTokenSrc = new CancellationTokenSource())
            {
                try
                {
                    foreach (string entityFile in entityFileList)
                    {
                        // 処理キャンセルの確認
                        _cancelTokenSrc.Token.ThrowIfCancellationRequested();

                        // エンティティ情報を生成
                        GenerateInfoSingle(entityFile);

                        // 進捗状況の更新
                        count++;
                        float percentage = count / (float)entityFileList.Count;
                        progress.Report(new ProgressInfo((int)(percentage * 100.0F), $"Check chunk... {count} / {entityFileList.Count} ({percentage.ToString("P")})"));
                    }

                    UpdateVillageInfo();
                    WriteEntitiesList(_entitiesList);
                }
                catch (OperationCanceledException)
                {
                    // 処理がキャンセルされた場合
                    progress.Report(new ProgressInfo(0, "処理をキャンセルしました。"));
                    return;
                }
            }

            progress.Report(new ProgressInfo(100, "処理が完了しました。"));
            return;
        }

        /// <summary>
        /// 処理キャンセル
        /// </summary>
        public void CancelGenerate()
        {
            if (_cancelTokenSrc?.IsCancellationRequested == false)
            {
                _cancelTokenSrc.Cancel();
            }

            return;
        }

        /// <summary>
        /// 村検索(名前で検索)
        /// </summary>
        /// <param name="villageName">村の名前</param>
        /// <returns>ヒットした村情報</returns>
        public VillageInfo? FindVillage(string villageName)
        {
            return VillageList.Find(v => v.VillageName == villageName);
        }

        /// <summary>
        /// 村検索(範囲で検索)
        /// </summary>
        /// <param name="baseRange">検索範囲</param>
        /// <returns>ヒットした村情報</returns>
        public List<VillageInfo> FindVillageForTargetRange(PointRange baseRange)
        {
            List<VillageInfo> vid = VillageList.Where(village =>
                baseRange.IsCollisionWithCircle(village.LeftTop, village.RightBottom)
            ).ToList();

            vid.Sort((v1, v2) => (int)(v1.GetDistanceToVillageCenter(baseRange.Point) - v2.GetDistanceToVillageCenter(baseRange.Point)));
            return vid;
        }

        /// <summary>
        /// 村人選択
        /// </summary>
        /// <param name="targetVillage">対象の村情報</param>
        /// <returns>対象の村に住んでいる村人情報</returns>
        public List<MCAVillagerInfo> SelectVillagers(VillageInfo? targetVillage)
        {
            // 対象の村がnullの場合は空のリストを返す
            if (targetVillage == null)
            {
                return new List<MCAVillagerInfo>();
            }

            // 対象の村に住んでいる村人をフィルタリング
            List<MCAVillagerInfo> villageFilter = VillagerList.Where(villager => villager.ResidenceVillageName == targetVillage.VillageName).ToList();

            // 住民リストに登録されていない村人をフィルタリング
            List<MCAVillagerInfo> noVillageFilter = VillagerList.Where(villager =>
                villager.ResidenceVillageName == "-" &&
                villager.CoordinatePos.X >= targetVillage.LeftTop.X &&
                villager.CoordinatePos.Y >= targetVillage.LeftTop.Y &&
                villager.CoordinatePos.Z >= targetVillage.LeftTop.Z &&
                villager.CoordinatePos.X <= targetVillage.RightBottom.X &&
                villager.CoordinatePos.Y <= targetVillage.RightBottom.Y &&
                villager.CoordinatePos.Z <= targetVillage.RightBottom.Z).ToList();

            // 住民リストに登録されていない村人を追加
            villageFilter.AddRange(noVillageFilter);
            return villageFilter;
        }

        /// <summary>
        /// サムネイル(顔)の生成
        /// </summary>
        /// <param name="data">村人情報</param>
        /// <returns>サムネイル</returns>
        public BitmapSource GenerateFaceThumb(in MCAVillagerInfo data)
        {
            return _skinGenerator.GenerateFaceThumb(data);
        }

        /// <summary>
        /// 対象村人のスキン出力
        /// </summary>
        /// <param name="outputBasePath">出力先のベースパス</param>
        /// <param name="villageName">村の名前</param>
        public void OutputSkin(string outputBasePath, string villageName)
        {
            // 出力先のディレクトリを作成
            string outputDir = Path.Combine(outputBasePath, villageName, "skins");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // 対象の村人情報を取得
            List<MCAVillagerInfo> villagers = SelectVillagers(FindVillage(villageName));

            // 村人のスキンを出力
            foreach (MCAVillagerInfo data in villagers)
            {
                _skinGenerator.OutputSkin(data, outputDir);
            }

            return;
        }

        /// <summary>
        /// 村人リストをCSV形式で出力する
        /// </summary>
        /// <param name="outputBasePath">出力先のベースパス</param>
        /// <param name="villageName">村の名前</param>
        public void OutputVillagerListCSV(string outputBasePath, string villageName)
        {
            // 出力先のディレクトリを作成
            string outputDir = Path.Combine(outputBasePath, villageName);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // 対象の村人情報を取得
            List<MCAVillagerInfo> villagers = SelectVillagers(FindVillage(villageName));

            // 村人の情報をCSV形式で出力
            CommonLib.WriteCsv<MCAVillagerInfo, MCAVillagerInfoMap>(Path.Combine(outputDir, "all_villagers.csv"), villagers);
            return;
        }

        /// <summary>
        /// 全ての村の情報をCSV形式で出力する
        /// </summary>
        public void OutputVillageInfo()
        {
            CommonLib.WriteCsv<VillageInfo, VillageInfoMap>(Path.Combine(_outputBasePath, "all_village_info.csv"), VillageList);
            return;
        }

        /// <summary>
        /// 村の基本情報を初期化する
        /// </summary>
        private void InitData()
        {
            // 村の基本情報の生成
            VillagerList.Clear();
            ReadEntitiesList();
            GenerateVillageInfo();
            return;
        }

        /// <summary>
        /// 村の基本情報の生成
        /// </summary>
        private void GenerateVillageInfo()
        {
            _regidentList.Clear();
            VillageList.Clear();

            // MCAの村情報を取得するために、mca_villages.datを読み込む
            NbtFile nbt = new NbtFile();
            nbt.LoadFromFile(Path.Combine(_savePath, "data", "mca_villages.dat"));
            NbtList villages = (NbtList)nbt.RootTag["data"]["villages"];

            // 村の情報を取得
            foreach (NbtTag village in villages)
            {
                AddResidentList((NbtCompound)village["residentNames"], village["name"].StringValue);
                AddVillageRange((NbtList)village["buildings"], village["name"].StringValue);
            }

            return;
        }

        /// <summary>
        /// 住民リスト追加
        /// </summary>
        /// <param name="villagers">村人リスト(NBTフォーマット)</param>
        /// <param name="villageName">村の名前</param>
        private void AddResidentList(in NbtCompound villagers, string villageName)
        {
            // 村人を住民リストに追加
            foreach (NbtTag villager in villagers)
            {
                _regidentList.Add(villager.Name, villageName);
            }

            return;
        }

        /// <summary>
        /// 村の範囲追加
        /// </summary>
        /// <param name="buildings">建物リスト(NBTフォーマット)</param>
        /// <param name="villageName">村の名前</param>
        private void AddVillageRange(NbtList buildings, string villageName)
        {
            // 村の範囲を決定するための初期値設定
            VillageInfo villageRange = new VillageInfo();
            villageRange.VillageName = villageName;
            villageRange.LeftTop = new Point3D(int.MaxValue, int.MaxValue, int.MaxValue);
            villageRange.RightBottom = new Point3D(int.MinValue, int.MinValue, int.MinValue);

            // 建物の座標から村の範囲を決定する
            foreach (NbtTag building in buildings)
            {
                villageRange.LeftTop.Min(building["pos0X"].IntValue, building["pos0Y"].IntValue, building["pos0Z"].IntValue);
                villageRange.RightBottom.Max(building["pos1X"].IntValue, building["pos1Y"].IntValue, building["pos1Z"].IntValue);
            }

            // 村の外側に32ブロック分のマージンを追加する
            villageRange.LeftTop.ShiftAll(-32);
            villageRange.RightBottom.ShiftAll(32);

            // 村の中心を取得
            villageRange.Center = CommonLib.GetBoxCenter(villageRange.LeftTop, villageRange.RightBottom);

            VillageList.Add(villageRange);
            return;
        }

        /// <summary>
        /// ファイル名からリージョン座標を取得
        /// </summary>
        /// <param name="entityFile">エンティティファイル名</param>
        /// <returns>リージョン座標</returns>
        private PointXZ GetRegionPosFromEntityFileName(string entityFile)
        {
            string[] splitRegion = Path.GetFileName(entityFile).Split('.');
            return new PointXZ(int.Parse(splitRegion[1]), int.Parse(splitRegion[2]));
        }

        /// <summary>
        /// エンティティファイル一覧読み込み
        /// </summary>
        private void ReadEntitiesList()
        {
            _entitiesList.Clear();
            string entitiesListPath = Path.Combine(_outputBasePath, "entities_list.json");
            if (File.Exists(entitiesListPath))
            {
                _entitiesList = CommonLib.ReadJson<Dictionary<string, string>>(entitiesListPath);
            }

            return;
        }

        /// <summary>
        /// エンティティファイル一覧出力
        /// </summary>
        /// <param name="entitiesList">エンティティファイル一覧</param>
        private void WriteEntitiesList(Dictionary<string, string> entitiesList)
        {
            string entitiesListPath = Path.Combine(_outputBasePath, "entities_list.json");
            CommonLib.WriteJson(entitiesListPath, entitiesList);
            return;
        }

        /// <summary>
        /// 単一のエンティティファイルから情報を生成する
        /// </summary>
        /// <param name="entityFile">エンティティファイル名</param>
        private void GenerateInfoSingle(string entityFile)
        {
            // entityファイルのタイムスタンプを取得
            string fileName = Path.GetFileName(entityFile);
            string? prevTimestamp = "0";
            _entitiesList.TryGetValue(fileName, out prevTimestamp);
            string nowTimestamp = File.GetLastWriteTime(entityFile).ToString("yyyyMMdd_HHmmss_fff");

            // リージョン内のエンティティデータ取得
            GetRegionEntityData(entityFile, prevTimestamp != nowTimestamp);

            // エンティティファイル一覧更新
            if (_entitiesList.ContainsKey(fileName))
            {
                _entitiesList[fileName] = nowTimestamp;
            }
            else
            {
                _entitiesList.Add(fileName, nowTimestamp);
            }

            return;
        }

        /// <summary>
        /// リージョン内のエンティティデータ取得
        /// </summary>
        /// <param name="entityFile">エンティティファイル名</param>
        /// <param name="isUpdate">更新フラグ</param>
        private void GetRegionEntityData(string entityFile, bool isUpdate)
        {
            // エンティティファイル名からリージョン座標を取得
            PointXZ regionPos = GetRegionPosFromEntityFileName(entityFile);
            string regionFileName = $"region_{regionPos.X}_{regionPos.Z}.json";
            string intermediatePath = Path.Combine(_regionResultBasePath, regionFileName);

            if (isUpdate)
            {
                // タイムスタンプが更新されているファイルだけ処理する
                VillagerList.AddRange(ProcessingRegionData(entityFile, regionPos, intermediatePath));
            }
            else
            {
                // タイムスタンプが更新されていない場合は、既存のデータを読み込む
                List<MCAVillagerInfo>? regionVillagerData = CommonLib.ReadJson<List<MCAVillagerInfo>>(intermediatePath);
                if (regionVillagerData != null)
                {
                    VillagerList.AddRange(regionVillagerData);
                }
            }

            return;
        }

        /// <summary>
        /// リージョンデータ処理
        /// </summary>
        /// <param name="entityFile">エンティティファイル名</param>
        /// <param name="regionPos">リージョン座標</param>
        /// <param name="intermediatePath">中間処理結果のパス</param>
        /// <returns>リージョン内のMCA村人リスト</returns>
        private List<MCAVillagerInfo> ProcessingRegionData(string entityFile, PointXZ regionPos, string intermediatePath)
        {
            List<MCAVillagerInfo> regionVillagerData = new List<MCAVillagerInfo>();
            MCRegion region = new MCRegion();
            region.ReadFile(entityFile);

            // チャンク毎に処理
            for (int chunkX = 0; chunkX < 32; chunkX++)
            {
                for (int chunkZ = 0; chunkZ < 32; chunkZ++)
                {
                    // チャンクデータ取得
                    NbtTag? chunk = region.GetChunkData(chunkX, chunkZ);
                    if (chunk == null)
                    {
                        continue;
                    }

                    // チャンク内のエンティティデータを処理
                    regionVillagerData.AddRange(ProcessingChunkEntityData(chunk, regionPos, new PointXZ(chunkX, chunkZ)));
                }
            }

            // 中間処理結果を保存
            CommonLib.WriteJson(intermediatePath, regionVillagerData);
            VillagerList.AddRange(regionVillagerData);
            return regionVillagerData;
        }

        /// <summary>
        /// チャンク内のエンティティを処理
        /// </summary>
        /// <param name="chunk">チャンクデータ(NBTフォーマット)</param>
        /// <param name="regionPos">リージョン座標</param>
        /// <param name="chunkPos">チャンク座標</param>
        /// <returns>チャンク内のMCA村人リスト</returns>
        private List<MCAVillagerInfo> ProcessingChunkEntityData(in NbtTag chunk, PointXZ regionPos, PointXZ chunkPos)
        {
            List<MCAVillagerInfo> chunkEntityData = new List<MCAVillagerInfo>();
            PointXZ worldChunkPos = new PointXZ(chunk["Position"].IntArrayValue[0], chunk["Position"].IntArrayValue[1]);

            foreach (NbtTag entity in (NbtList)chunk["Entities"])
            {
                switch (entity["id"].StringValue)
                {
                    // ボートに乗っているエンティティ
                    case "minecraft:boat":
                        foreach (NbtTag passenger in (NbtList)entity["Passengers"])
                        {
                            if (passenger["id"].StringValue == "mca:male_villager" || passenger["id"].StringValue == "mca:female_villager")
                            {
                                chunkEntityData.Add(AddVillagerData(passenger, regionPos, chunkPos, worldChunkPos));
                            }
                        }

                        break;

                    // MCA村人
                    case "mca:male_villager":
                    case "mca:female_villager":
                        chunkEntityData.Add(AddVillagerData(entity, regionPos, chunkPos, worldChunkPos));
                        break;
                }
            }

            return chunkEntityData;
        }

        /// <summary>
        /// MCA村人追加
        /// </summary>
        /// <param name="entity">エンティティデータ(NBTフォーマット)</param>
        /// <param name="regionPos">リージョン座標</param>
        /// <param name="chunkPos">チャンク座標</param>
        /// <param name="worldChunkPos">ワールドチャンク座標</param>
        /// <returns></returns>
        private MCAVillagerInfo AddVillagerData(in NbtTag entity, PointXZ regionPos, PointXZ chunkPos, PointXZ worldChunkPos)
        {
            MCAVillagerInfo villager = new MCAVillagerInfo(entity, _playerUUID, regionPos, chunkPos, worldChunkPos);
            villager.RecordResidenceVillageName(_regidentList);
            return villager;
        }

        /// <summary>
        /// 村の基本情報を更新
        /// </summary>
        private void UpdateVillageInfo()
        {
            foreach (VillageInfo vid in VillageList)
            {
                // 村人集計
                List<MCAVillagerInfo> villageFilter = SelectVillagers(vid);
                List<MCAVillagerInfo> mcaVillagers = villageFilter.Where(villager =>
                    villager.Profession != "メイド" && villager.Profession != "無法者").ToList();

                // 人口取得
                vid.Population = mcaVillagers.Count;
                vid.Guards = mcaVillagers.Where(villager =>
                    villager.Profession == "衛兵" || villager.Profession == "弓兵").Count();
                if (vid.Population > 0)
                {
                    vid.GuardRate = (vid.Guards / (float)vid.Population).ToString("P");
                }
            }

            // 人口でソート
            VillageList.Sort((v1, v2) => v1.VillageName.CompareTo(v2.VillageName));
            //VillageDataList.Sort((v1, v2) => v2.Population - v1.Population);
            return;
        }
    }
}
