using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCALib.Data
{
    /// <summary>
    /// VillageInfoクラスのCSVマッピングを定義するクラス
    /// </summary>
    public class VillageInfoMap : ClassMap<VillageInfo>
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VillageInfoMap()
        {
            Map(m => m.VillageName).Index(0).Name("village_name");
            Map(m => m.LeftTop.X).Index(1).Name("left_top_x");
            Map(m => m.LeftTop.Y).Index(2).Name("left_top_y");
            Map(m => m.LeftTop.Z).Index(3).Name("left_top_z");
            Map(m => m.RightBottom.X).Index(4).Name("right_bottom_x");
            Map(m => m.RightBottom.Y).Index(5).Name("right_bottom_y");
            Map(m => m.RightBottom.Z).Index(6).Name("right_bottom_z");
            Map(m => m.Center.X).Index(7).Name("center_x");
            Map(m => m.Center.Y).Index(8).Name("center_y");
            Map(m => m.Center.Z).Index(9).Name("center_z");
            Map(m => m.Population).Index(10).Name("population");
            Map(m => m.Guards).Index(11).Name("guards");
            Map(m => m.GuardRate).Index(12).Name("guard_rate");
        }
    }
}
