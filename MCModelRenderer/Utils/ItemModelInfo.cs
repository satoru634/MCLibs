using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCToolsCommonLib.Common;
using MCToolsCommonLib.Utils;

namespace MCModelRenderer.Utils
{
    /// <summary>
    /// 1.21.4以降のアイテムモデルの情報を格納するクラス
    /// </summary>
    public class ItemModelInfo : JarLoader, IDisposable
    {
        /// <summary>
        /// アイテムIDを格納するプロパティ
        /// </summary>
        private string _itemID;

        private string _itemModelPath;

        private Dictionary<string, object> _itemModel;

        /// <summary>
        /// リソースの解放状態を示すフラグ。
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="itemID"></param>
        /// <param name="jarPath"></param>
        /// <param name="languageCode"></param>
        /// <param name="version"></param>
        public ItemModelInfo(string itemID, string jarPath, string languageCode, string version) : base(jarPath, languageCode, version)
        {
            _itemID = itemID;
            _itemModelPath = $"assets/{NameSpace}/items/{_itemID.Replace($"{NameSpace}:", "")}.json";
            _itemModel = new Dictionary<string, object>();
            _itemModel = JsonLoad<Dictionary<string, object>>(_itemModelPath);
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public ItemModelInfo()
        {
            _itemID = "";
            _itemModelPath = "";
            _itemModel = new Dictionary<string, object>();
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

                _itemModel.Clear();

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
        ~ItemModelInfo()
        {
            Dispose(false);
        }

        /// <summary>
        /// モデルのパスを取得
        /// </summary>
        /// <returns></returns>
        public string GetModelPath()
        {
            // モデルの情報が存在しない場合は空文字列を返す
            if (!_itemModel.ContainsKey("model"))
            {
                return "";
            }

            // モデルのIDを取得
            string strModel = GetModelID(_itemModel["model"].ToString());

            // モデルのIDが空文字列の場合は空文字列を返す
            if (strModel == "")
            {
                return "";
            }

            // モデルのパスを返す
            return $"assets/{NameSpace}/models/{strModel.Replace($"{NameSpace}:", "")}.json";
        }

        /// <summary>
        /// モデルのIDを取得
        /// </summary>
        /// <param name="strJson">jsonデータ</param>
        /// <returns>モデルID</returns>
        private string GetModelID(string? strJson)
        {
            var model = CommonLib.DeserializeJson<Dictionary<string, object>>(strJson);

            // アイテムモデルのタイプに応じて処理を分岐
            string strModel;
            switch (model["type"].ToString())
            {
                case "minecraft:model":
                    strModel = model["model"].ToString() ?? "";
                    break;
                case "minecraft:special":
                    strModel = model["base"].ToString() ?? "";
                    break;
                case "minecraft:select":
                    strModel = GetModelID(model["fallback"].ToString());
                    break;
                case "minecraft:condition":
                    strModel = GetModelID(model["on_false"].ToString());
                    break;
                case "minecraft:range_dispatch":
                    strModel = GetRangeModelID(model["entries"].ToString(), 0.0);
                    break;
                default:
                    return "";
            }

            return strModel;
        }

        /// <summary>
        /// 指定された閾値に一致するモデルIDを取得する。
        /// </summary>
        /// <param name="strJson">jsonデータ</param>
        /// <param name="threshold">閾値</param>
        /// <returns>モデルID</returns>
        private string GetRangeModelID(string? strJson, double threshold)
        {
            var entries = CommonLib.DeserializeJson<List<object>>(strJson);
            foreach (var entry in entries)
            {
                var entryDict = CommonLib.DeserializeJson<Dictionary<string, object>>(entry.ToString());
                double targetThreshold = ConvertJsonValue.ConvertDouble(entryDict["threshold"]);
                if (CommonLib.IsEqual(targetThreshold, threshold))
                {
                    return GetModelID(entryDict["model"].ToString());
                }
            }

            return "";
        }
    }
}
