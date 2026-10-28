using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.Common
{
    public class ItemBaseViewModel<T>
    {
        /// <summary>
        /// アイテムのリスト
        /// </summary>
        public List<T> ItemList { get; set; }

        /// <summary>
        /// 選択されているアイテムのインデックス
        /// </summary>
        public int SelectedIndex { get; set; }

        /// <summary>
        /// アイテムの選択を有効にするかどうかのフラグ
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public ItemBaseViewModel()
        {
            ItemList = new List<T>();
            SelectedIndex = -1;
            Enable = false;
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="data"></param>
        public ItemBaseViewModel(ItemBaseViewModel<T> data)
        {
            ItemList = data.ItemList;
            SelectedIndex = data.SelectedIndex;
            Enable = data.Enable;
        }

        /// <summary>
        /// アイテムのリストを初期化し、選択されたアイテムを設定する
        /// </summary>
        /// <param name="items">アイテムのリスト</param>
        /// <param name="selectedText">選択中のアイテム</param>
        public virtual void Init(List<T> items, T selectedText)
        {
            if (items.Count == 0)
            {
                SelectedIndex = -1;
                return;
            }

            ItemList = items;
            SelectedIndex = items.FindIndex(item => Equals(item, selectedText));
            if (SelectedIndex == -1)
            {
                SelectedIndex = 0;
            }

            Enable = true;
            return;
        }

        /// <summary>
        /// アイテムを追加
        /// </summary>
        /// <param name="data">追加するアイテム</param>
        public virtual void Add(T data)
        {
            ItemList.Add(data);
            return;
        }

        /// <summary>
        /// アイテムを削除
        /// </summary>
        public virtual void Clear()
        {
            ItemList.Clear();
            SelectedIndex = -1;
            Enable = false;
        }
    }
}
