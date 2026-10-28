using CsvHelper.Configuration;
using fNbt;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MCToolsCommonLib.BaseData;
using MCToolsCommonLib.Common;

namespace MCALib.Data
{
    /// <summary>
    /// MCA村人情報クラス
    /// </summary>
    public class MCAVillagerInfo : EntityInfoBase
    {
        /// <summary>
        /// 性別
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// 好感度
        /// </summary>
        public Dictionary<string, int> Hearts { get; set; }

        /// <summary>
        /// 気分
        /// </summary>
        public int Mood { get; set; }

        /// <summary>
        /// 年齢
        /// </summary>
        public CommonDefine.AgeState Age { get; set; }

        /// <summary>
        /// 赤ちゃんを持っているかどうか
        /// </summary>
        public bool HasBaby { get; set; }

        /// <summary>
        /// 職業
        /// </summary>
        public string Profession { get; set; }

        /// <summary>
        /// 村人と交易済みかどうか
        /// </summary>
        public bool Traded { get; set; }

        /// <summary>
        /// 住んでいる村の名前
        /// </summary>
        public string ResidenceVillageName { get; set; }

        /// <summary>
        /// 服装
        /// </summary>
        public string Clothes { get; set; }

        /// <summary>
        /// 髪のパターン
        /// </summary>
        public string Hair { get; set; }

        /// <summary>
        /// ユーメラニン(髪の色を決定する要素)
        /// </summary>
        public float GeneEumelanin { get; set; }

        /// <summary>
        /// 顔のパターン
        /// </summary>
        public float GeneFace { get; set; }

        /// <summary>
        /// ヘモグロビン(肌の色を決定する要素)
        /// </summary>
        public float GeneHemoglobin { get; set; }

        /// <summary>
        /// メラニン(肌の色を決定する要素)
        /// </summary>
        public float GeneMelanin { get; set; }

        /// <summary>
        /// フェオメラニン(髪の色を決定する要素)
        /// </summary>
        public float GenePheomelanin { get; set; }

        /// <summary>
        /// 肌のパターン
        /// </summary>
        public float GeneSkin { get; set; }

        /// <summary>
        /// 髪の色(青)
        /// </summary>
        public float HairColorBlue { get; set; }

        /// <summary>
        /// 髪の色(緑)
        /// </summary>
        public float HairColorGreen { get; set; }

        /// <summary>
        /// 髪の色(赤)
        /// </summary>
        public float HairColorRed { get; set; }

        /// <summary>
        /// アルビノ
        /// </summary>
        public float Albinism { get; set; }

        /// <summary>
        /// オッドアイ
        /// </summary>
        public bool Hetero { get; set; }

        /// <summary>
        /// 検索用キー
        /// </summary>
        public string SearchKey { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public MCAVillagerInfo() : base()
        {
            Gender = "";
            Hearts = new Dictionary<string, int>();
            Mood = 15;
            Age =  CommonDefine.AgeState.Adult;
            HasBaby = false;
            Profession = "";
            Traded = false;
            ResidenceVillageName = "-";
            Clothes = "-";
            Hair = "-";
            GeneEumelanin = 0.0F;
            GeneFace = 0.0F;
            GeneHemoglobin = 0.0F;
            GeneMelanin = 0.0F;
            GenePheomelanin = 0.0F;
            GeneSkin = 0.0F;
            HairColorBlue = 0.0F;
            HairColorGreen = 0.0F;
            HairColorRed = 0.0F;
            Albinism = 1.0F;
            Hetero = false;
            SearchKey = "";
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="info"></param>
        public MCAVillagerInfo(MCAVillagerInfo info)
        {
            Gender = info.Gender;
            Hearts = new Dictionary<string, int>(info.Hearts);
            Mood = info.Mood;
            Age = info.Age;
            HasBaby = info.HasBaby;
            Profession = info.Profession;
            Traded = info.Traded;
            ResidenceVillageName = info.ResidenceVillageName;
            Clothes = info.Clothes;
            Hair = info.Hair;
            GeneEumelanin = info.GeneEumelanin;
            GeneFace = info.GeneFace;
            GeneHemoglobin = info.GeneHemoglobin;
            GeneMelanin = info.GeneMelanin;
            GenePheomelanin = info.GenePheomelanin;
            GeneSkin = info.GeneSkin;
            HairColorBlue = info.HairColorBlue;
            HairColorGreen = info.HairColorGreen;
            HairColorRed = info.HairColorRed;
            Albinism = info.Albinism;
            Hetero = info.Hetero;
            SearchKey = info.SearchKey;
            UUID = info.UUID;
            Name = info.Name;
            CoordinatePos = new Point3D(info.CoordinatePos);
            RegionPos = new PointXZ(info.RegionPos);
            ChunkPos = new PointXZ(info.ChunkPos);
            WorldChunkPos = new PointXZ(info.WorldChunkPos);
            ItemInfo = new List<InventoryItem>(info.ItemInfo);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="entity">エンティティデータ(NBTフォーマット)</param>
        /// <param name="playerUUID">プレイヤーのUUID</param>
        /// <param name="regionPos">リージョン座標</param>
        /// <param name="chunkPos">チャンク座標</param>
        /// <param name="worldChunkPos">ワールドチャンク座標</param>
        public MCAVillagerInfo(in NbtTag entity, string playerUUID, PointXZ regionPos, PointXZ chunkPos, PointXZ worldChunkPos) : this()
        {
            // 村人の座標を初期化
            CoordinatePos = new Point3D(
                (int)Math.Round(entity["Pos"][0].DoubleValue),
                (int)Math.Round(entity["Pos"][1].DoubleValue),
                (int)Math.Round(entity["Pos"][2].DoubleValue));

            // リージョン、チャンク、ワールドチャンクの座標を初期化
            RegionPos = regionPos;
            ChunkPos = chunkPos;
            WorldChunkPos = worldChunkPos;

            // 村人情報を初期化
            InitVillagerData(entity, playerUUID);

            // 村人のインベントリーを取得
            GetInventory((NbtList)entity["Inventory"]);

            // 検索キーを生成
            GenerateSearchKey();
        }

        /// <summary>
        /// 住んでいる村の名前を登録する。
        /// </summary>
        /// <param name="regidentList">住民リスト</param>
        public void RecordResidenceVillageName(Dictionary<string, string> regidentList)
        {
            // 住んでいる村の名前を登録する
            if (regidentList.ContainsKey(UUID))
            {
                ResidenceVillageName = regidentList[UUID];
            }

            return;
        }

        /// <summary>
        /// 村人の要約情報を取得する。
        /// </summary>
        /// <param name="playerUUID">プレイヤーのUUID</param>
        /// <returns>村人の要約情報</returns>
        public Dictionary<string, string> GetSummary(string playerUUID)
        {
            Dictionary<string, string> summary = new Dictionary<string, string>
            {
                { "VillagerName", Name },
                { "Gender", CommonDefine.GenderString[Gender] },
                { "UUID", UUID },
                { "Hearts", Hearts[playerUUID].ToString() },
                { "Mood", Mood.ToString() },
                { "Profession", Profession },
                { "ResidenceVillageName", ResidenceVillageName },
                { "Position", GeneratePositionString() },
            };

            return summary;
        }

        /// <summary>
        /// 村人のデータを初期化する。
        /// </summary>
        /// <param name="entity">エンティティデータ(NBTフォーマット)</param>
        /// <param name="playerUUID">プレイヤーのUUID</param>
        private void InitVillagerData(in NbtTag entity, string playerUUID)
        {
            // 村人の基本情報を取得
            Name = entity["villagerName"].StringValue;
            Gender = entity["gender"].IntValue == 1 ? "male" : "female";
            UUID = CommonLib.ConvertUUID(entity["UUID"].IntArrayValue);
            NbtCompound memories = (NbtCompound)entity["memories"];
            if (memories.Count > 0)
            {
                Hearts.Add(playerUUID, memories[playerUUID]["hearts"].IntValue);
            }

            Mood = entity["mood"].IntValue;
            Age = (CommonDefine.AgeState)entity["ageState"].IntValue;
            HasBaby = entity["hasBaby"].IntValue == 1 ? true : false;
            Traded = entity["Xp"].IntValue > 0 ? true : false;

            // 村人の職業を取得
            try
            {
                Profession = CommonDefine.VillagerProfession[entity["VillagerData"]["profession"].StringValue];
            }
            catch
            {
                Profession = "未知の職業";
            }

            // 村人のスキンに関する情報を取得
            Clothes = entity["clothes"].StringValue;
            Hair = entity["hair"].StringValue;
            GeneEumelanin = entity["gene_eumelanin"].FloatValue;
            GeneFace = entity["gene_face"].FloatValue;
            GeneHemoglobin = entity["gene_hemoglobin"].FloatValue;
            GeneMelanin = entity["gene_melanin"].FloatValue;
            GenePheomelanin = entity["gene_pheomelanin"].FloatValue;
            GeneSkin = entity["gene_skin"].FloatValue;
            HairColorBlue = entity["hair_color_blue"].FloatValue;
            HairColorGreen = entity["hair_color_green"].FloatValue;
            HairColorRed = entity["hair_color_red"].FloatValue;

            // 村人の特性からアルビノとオッドアイを取得
            foreach (NbtTag traits in (NbtCompound)entity["traits"])
            {
                switch (traits.Name)
                {
                    case "ALBINISM":
                        Albinism = 0.1F;
                        break;

                    case "HETEROCHROMIA":
                        Hetero = true;
                        break;
                }
            }

            return;
        }

        /// <summary>
        /// 村人のインベントリーを取得する。
        /// </summary>
        /// <param name="inventory">インベントリー(NBTフォーマット)</param>
        private void GetInventory(in NbtList inventory)
        {
            foreach (NbtTag tag in inventory)
            {
                (string itemName, int itemCount) = CommonLib.GetItemInfo(tag);
                ItemInfo.Add(new InventoryItem(itemName, itemCount));
            }

            return;
        }

        /// <summary>
        /// 村人の検索キーを生成する。
        /// </summary>
        private void GenerateSearchKey()
        {
            SearchKey = $"<Gender:{Gender}>";
            SearchKey += $"<Age:{CommonDefine.AgeStateString[Age]}>";
            SearchKey += $"<HasBaby:{HasBaby}>";
            SearchKey += $"<Traded:{Traded}>";
            SearchKey += $"<Profession:{Profession}>";
            return;
        }

        /// <summary>
        /// 村人の位置情報を文字列として生成する。
        /// </summary>
        /// <returns>位置情報</returns>
        private string GeneratePositionString()
        {
            string pos = $"R({RegionPos.ToString()}), ";
            pos += $"C({WorldChunkPos.ToString()}), ";
            pos += $"B({CoordinatePos.ToString()})";
            return pos;
        }
    }
}
