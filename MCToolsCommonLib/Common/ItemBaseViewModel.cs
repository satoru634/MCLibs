using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.Common
{
    public class ItemBaseViewModel<T>
    {
        public List<T> ItemList { get; set; }
        public int SelectedIndex { get; set; }
        public bool Enable { get; set; }

        public ItemBaseViewModel()
        {
            ItemList = new List<T>();
            SelectedIndex = -1;
            Enable = false;
        }

        public ItemBaseViewModel(ItemBaseViewModel<T> data)
        {
            ItemList = data.ItemList;
            SelectedIndex = data.SelectedIndex;
            Enable = data.Enable;
        }

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

        public virtual void Add(T data)
        {
            ItemList.Add(data);
            return;
        }

        public virtual void Clear()
        {
            ItemList.Clear();
            SelectedIndex = -1;
            Enable = false;
        }
    }
}
