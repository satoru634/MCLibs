using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.BaseData
{
    /// <summary>
    /// エンティティ情報のベースクラス
    /// </summary>
    public class EntityInfoBase
    {
        /// <summary>
        /// エンティティの名前
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// エンティティのUUID
        /// </summary>
        public string UUID { get; set; }

        /// <summary>
        /// エンティティの座標(ワールド座標)
        /// </summary>
        public Point3D CoordinatePos { get; set; }

        /// <summary>
        /// エンティティの座標(リージョン)
        /// </summary>
        public PointXZ RegionPos { get; set; }

        /// <summary>
        /// エンティティの座標(チャンク)
        /// </summary>
        public PointXZ ChunkPos { get; set; }

        /// <summary>
        /// エンティティの座標(ワールドチャンク)
        /// </summary>
        public PointXZ WorldChunkPos { get; set; }

        /// <summary>
        /// エンティティのインベントリー
        /// </summary>
        public List<InventoryItem> ItemInfo { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public EntityInfoBase()
        {
            Name = "";
            UUID = "";
            CoordinatePos = new Point3D();
            RegionPos = new PointXZ();
            ChunkPos = new PointXZ();
            WorldChunkPos = new PointXZ();
            ItemInfo = new List<InventoryItem>();
        }
    }
}
