using fNbt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.Common
{
    /// <summary>
    /// 設定ファイルを管理するためのクラス
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Setting<T> where T : new()
    {
        /// <summary>
        /// 設定データを格納するプロパティ
        /// </summary>
        public T Data { get; set; } = new T();

        /// <summary>
        /// 設定ファイルのパスを格納するプロパティ
        /// </summary>
        private string _settingPath { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="settingPath">設定ファイルのパス</param>
        /// <param name="onLoad">設定ファイルを読み込むかどうかのフラグ</param>
        public Setting(string settingPath, bool onLoad = true)
        {
            _settingPath = settingPath;
            if (onLoad)
            {
                Load();
            }

            return;
        }

        /// <summary>
        /// 設定ファイルを読み込む
        /// </summary>
        public void Load()
        {
            if (!File.Exists(_settingPath))
            {
                Save();
                return;
            }

            Data = CommonLib.ReadJson<T>(_settingPath);
            return;
        }

        /// <summary>
        /// 設定ファイルにデータを保存する
        /// </summary>
        public void Save()
        {
            CommonLib.WriteJson(_settingPath, Data);
            return;
        }
    }
}
