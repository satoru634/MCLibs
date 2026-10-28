using MCToolsCommonLib.BaseData;
using MCToolsCommonLib.Common;

namespace MCALib.Data
{
    /// <summary>
    /// 村の情報を保持するクラス
    /// </summary>
    public class VillageInfo
    {
        /// <summary>
        /// 村の名前
        /// </summary>
        public string VillageName { get; set; }

        /// <summary>
        /// 村の左上の座標
        /// </summary>
        public Point3D LeftTop { get; set; }

        /// <summary>
        /// 村の右下の座標
        /// </summary>
        public Point3D RightBottom { get; set; }

        /// <summary>
        /// 村の中心の座標
        /// </summary>
        public Point3D Center { get; set; }

        /// <summary>
        /// 村の人口
        /// </summary>
        public int Population { get; set; }

        /// <summary>
        /// 村の衛兵の数
        /// </summary>
        public int Guards { get; set; }

        /// <summary>
        /// 村の衛兵の割合（例: "0.00%"）
        /// </summary>
        public string GuardRate { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VillageInfo()
        {
            VillageName = "";
            LeftTop = new Point3D();
            RightBottom = new Point3D();
            Center = new Point3D();
            Population = 0;
            Guards = 0;
            GuardRate = "0.00%";
        }

        /// <summary>
        /// 村の情報を要約して辞書形式で返すメソッド
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetSummary()
        {
            Dictionary<string, string> summary = new Dictionary<string, string>
            {
                { "VillageName", VillageName },
                { "Population", Population.ToString() },
                { "Guards", Guards.ToString() },
                { "GuardRate", GuardRate.ToString() },
                { "Position", Center.ToString() },
            };

            return summary;
        }

        /// <summary>
        /// 指定した座標から村の中心までの距離を計算するメソッド
        /// </summary>
        /// <param name="point">座標(X,Z)</param>
        /// <returns>村の中心までの距離</returns>
        public double GetDistanceToVillageCenter(PointXZ point)
        {
            return CommonLib.CalcDistance2D(point, Center);
        }
    }
}
