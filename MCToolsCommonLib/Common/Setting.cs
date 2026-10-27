using fNbt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.Common
{
    public class Setting<T> where T : new()
    {
        public T Data { get; set; } = new T();

        private string SettingPath { get; set; }

        public Setting(string settingPath, bool onLoad = true)
        {
            SettingPath = settingPath;
            if (onLoad)
            {
                Load();
            }

            return;
        }

        public void Load()
        {
            if (!File.Exists(SettingPath))
            {
                Save();
                return;
            }

            Data = CommonLib.ReadJson<T>(SettingPath);
            return;
        }

        public void Save()
        {
            CommonLib.WriteJson(SettingPath, Data);
            return;
        }
    }
}
