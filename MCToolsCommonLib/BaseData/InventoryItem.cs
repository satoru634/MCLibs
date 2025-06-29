using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.BaseData
{
    /// <summary>
    /// インベントリーアイテム情報クラス
    /// </summary>
    public class InventoryItem
    {
        /// <summary>
        /// アイテム名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// アイテム数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public InventoryItem()
        {
            Name = "";
            Count = 0;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">アイテム名</param>
        /// <param name="count">アイテム数</param>
        public InventoryItem(string name, int count)
        {
            Name = name;
            Count = count;
        }
    }
}
