using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCModelRenderer.Utils
{
    public class ItemInfo
    {
        /// <summary>
        /// アイテムタイプ
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// アイテム名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public ItemInfo(string type, string name)
        {
            Type = type;
            Name = name;
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public ItemInfo()
        {
            Type = "";
            Name = "";
        }
    }
}
