using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCALib.Data
{
    /// <summary>
    /// MCA用共通定義クラス
    /// </summary>
    public class CommonDefine
    {
        /// <summary>
        /// 村人の職業定義
        /// </summary>
        public static readonly Dictionary<string, string> VillagerProfession = new Dictionary<string, string>()
        {
            {"minecraft:none", "無職"},
            {"minecraft:farmer", "農民"},
            {"minecraft:fisherman", "釣り人"},
            {"minecraft:shepherd", "羊飼い"},
            {"minecraft:fletcher", "矢師"},
            {"minecraft:librarian", "司書"},
            {"minecraft:cartographer", "製図家"},
            {"minecraft:cleric", "聖職者"},
            {"minecraft:armorer", "防具鍛冶"},
            {"minecraft:weaponsmith", "武器鍛冶"},
            {"minecraft:toolsmith", "道具鍛冶"},
            {"minecraft:butcher", "肉屋"},
            {"minecraft:leatherworker", "革細工師"},
            {"minecraft:mason", "石工"},
            {"minecraft:nitwit", "ニート"},
            {"mca:none", "無職"},
            {"mca:outlaw", "無法者"},
            {"mca:jeweler", "宝石商"},
            {"mca:pillager", "略奪者"},
            {"mca:miner", "坑夫"},
            {"mca:baker", "パン屋"},
            {"mca:guard", "衛兵"},
            {"mca:warrior", "戦士"},
            {"mca:archer", "弓兵"},
            {"mca:adventurer", "冒険家"},
            {"mca:mercenary", "傭兵"},
            {"mca:cultist", "カルト信者"},
            {"mca:child", "子ども"},
            {"chefsdelight:delightcook", "料理人"},
            {"chefsdelight:delightchef", "シェフ"},
            {"morevillagers:oceanographer", "海洋学者"},
            {"morevillagers:netherian", "怪異学者"},
            {"morevillagers:woodworker", "林業家"},
            {"morevillagers:enderian", "エンド研究家"},
            {"morevillagers:engineer", "エンジニア"},
            {"morevillagers:florist", "花屋"},
            {"morevillagers:hunter", "猟師"},
            {"morevillagers:miner", "鉱夫"},
            {"villagersplus:alchemist", "錬金術師"},
            {"villagersplus:occultist", "オカルティスト"},
            {"villagersplus:oceanographer", "海洋学者"},
            {"villagersplus:horticulturist", "園芸家"},
            {"brewery:brewer", "醸造家"}
        };

        /// <summary>
        /// エンティティの種類定義
        /// </summary>
        public enum EntityType
        {
            MCAVillager = 0,
            OtherEntity = 1
        }

        /// <summary>
        /// 年齢状態定義
        /// </summary>
        public enum AgeState
        {
            Baby = 1,
            Toddler = 2,
            Child = 3,
            Teen = 4,
            Adult = 5
        }

        /// <summary>
        /// 性別定義
        /// </summary>
        public static readonly Dictionary<string, string> GenderString = new Dictionary<string, string>
        {
            {"male", "男"},
            {"female", "女"}
        };

        /// <summary>
        /// 年齢状態の文字列表現定義
        /// </summary>
        public static readonly Dictionary<AgeState, string> AgeStateString = new Dictionary<AgeState, string>
        {
            {AgeState.Baby, "赤ちゃん"},
            {AgeState.Toddler, "幼児"},
            {AgeState.Child, "子供"},
            {AgeState.Teen, "思春期"},
            {AgeState.Adult, "大人"}
        };

        /// <summary>
        /// スキンの各部位の位置定義
        /// </summary>
        public static readonly Dictionary<string, OpenCvSharp.Rect> SkinFrontPositions = new Dictionary<string, OpenCvSharp.Rect>
        {
            {"head", new OpenCvSharp.Rect(8, 8, 8, 8)},
            {"head_armor", new OpenCvSharp.Rect(40, 8, 8, 8)},
            {"body", new OpenCvSharp.Rect(20, 20, 8, 12)},
            {"body_armor", new OpenCvSharp.Rect(20, 36, 8, 12)},
            {"right_arm", new OpenCvSharp.Rect(48, 32, 4, 12)},
            {"right_arm_armor", new OpenCvSharp.Rect(48, 48, 4, 12)},
            {"left_arm", new OpenCvSharp.Rect(36, 52, 4, 12)},
            {"left_arm_armor", new OpenCvSharp.Rect(52, 52, 4, 12)},
            {"right_leg", new OpenCvSharp.Rect(4, 20, 4, 12)},
            {"right_leg_armor", new OpenCvSharp.Rect(4, 36, 4, 12)},
            {"left_leg", new OpenCvSharp.Rect(20, 52, 4, 12)},
            {"left_leg_armor", new OpenCvSharp.Rect(4, 52, 4, 12)},
        };

        /// <summary>
        /// MCA村人のインベントリサイズ定義
        /// </summary>
        public static readonly int MaxInventorySize = 27;

        /// <summary>
        /// MCA村人の顔のパーツ数定義
        /// </summary>
        public static readonly int MCAFaceCount = 22;
    }
}
