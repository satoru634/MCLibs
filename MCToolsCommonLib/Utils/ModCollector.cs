using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MCToolsCommonLib.Utils
{
    public class ModCollector : IDisposable
    {
        /// <summary>
        /// MODのベースパスを格納するプロパティ
        /// </summary>
        private string _modBasePath { get; set; }

        /// <summary>
        /// 言語設定を保持するプロパティ
        /// </summary>
        private string _languageCode { get; set; }

        /// <summary>
        /// MODのリストを格納する辞書
        /// </summary>
        private Dictionary<string, string> _modList { get; set; }

        /// <summary>
        /// リソースの解放状態を示すフラグ
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public ModCollector() : base ()
        {
            _modBasePath = "";
            _languageCode = "";
            _modList = new Dictionary<string, string>();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modBasePath">MODファイルがあるベースパス</param>
        /// <param name="languageCode">言語コード</param>
        /// <param name="version">バージョン</param>
        public ModCollector(string modBasePath, string languageCode) : this()
        {
            _modBasePath = modBasePath;
            _languageCode = languageCode;
            GetModList();
        }

        /// <summary>
        /// リソースを解放するためのメソッド。
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed == false)
            {
                _disposed = true;
                _modList.Clear();

                if (disposing == true)
                {
                    GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>
        /// リソースを解放するためのメソッド。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// デストラクタ。
        /// </summary>
        ~ModCollector()
        {
            Dispose(false);
        }

        /// <summary>
        /// 指定された名前空間に対応するMODのパスを取得する
        /// </summary>
        /// <param name="nameSpace">名前空間</param>
        /// <returns>名前空間に対応するMODパス</returns>
        public string GetModPath(string nameSpace)
        {
            // MODのリストが空の場合は空文字列を返す
            if (_modList.Count == 0)
            {
                return "";
            }

            // 指定された名前空間に対応するMODのパスを取得
            if (_modList.ContainsKey(nameSpace))
            {
                return _modList[nameSpace];
            }

            return "";
        }

        /// <summary>
        /// MODのリストを取得
        /// </summary>
        private void GetModList()
        {
            // MODのベースパスが設定されていない、または存在しない場合は何もしない
            if ((string.IsNullOrEmpty(_modBasePath)) || (!Path.Exists(_modBasePath)))
            {
                return;
            }

            // MODのベースパスが存在する場合、ディレクトリ内のすべてのJARファイルを取得
            foreach (var modFile in Directory.GetFiles(_modBasePath, "*.jar"))
            {
                // JARファイルの名前空間を取得し、辞書に追加
                JarLoader jarLoader = new JarLoader(modFile, _languageCode);
                string nameSpace = jarLoader.GetNameSpace();
                _modList.Add(nameSpace, modFile);
            }

            return;
        }
    }
}
