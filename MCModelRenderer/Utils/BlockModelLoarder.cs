using MCToolsCommonLib.Common;
using MCToolsCommonLib.Utils;
using MCModelRenderer.MCModels;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Globalization;
using System.IO;
using System.Reflection.Metadata.Ecma335;

namespace MCModelRenderer.Utils
{
    /// <summary>
    /// Minecraftのブロックモデルをロードするためのクラス。
    /// Jarファイルからモデルデータやテクスチャを読み込み、BlockModelオブジェクトとして管理します。
    /// </summary>
    public class BlockModelLoarder : JarLoader, IDisposable
    {
        /// <summary>
        /// アイテムIDとそのタイプ（block/item）のマッピング
        /// </summary>
        private Dictionary<string, ItemInfo> _itemTypeList;

        /// <summary>
        /// アイテムIDとその名前部分のマッピング
        /// </summary>
        private Dictionary<string, string> _itemList;

        /// <summary>
        /// ブロックモデルのカラーパレットセットのリスト
        /// </summary>
        private Dictionary<string, Dictionary<string, object>> _colorSetList;

        /// <summary>
        /// アイテムモデル情報を保持するためのフィールド。
        /// </summary>
        private ItemModelInfo _itemModelInfo;

        /// <summary>
        /// リソースの解放状態を示すフラグ。
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// 利用可能なアイテムIDのリスト
        /// </summary>
        public List<string> ItemIDList { get; }

        /// <summary>
        /// 利用可能なアイテム名のリスト
        /// </summary>
        public List<string> ItemNameList { get; }

        /// <summary>
        /// 現在のブロックモデルを保持するプロパティ
        /// </summary>
        public BlockModel Model { get; private set; }

        /// <summary>
        /// 現在選択されているアイテムID
        /// </summary>
        public string ItemID { get; private set; }

        /// <summary>
        /// 現在選択されているアイテム名
        /// </summary>
        public string ItemName { get; private set; }

        public bool ModelEnabled { get; private set; }

        /// <summary>
        /// 指定されたJarファイルパスを使用してBlockModelLoarderを初期化します。
        /// </summary>
        /// <param name="jarPath">Jarファイルのパス。</param>
        public BlockModelLoarder(string jarPath, string languageCode, string version) : base(jarPath, languageCode, version)
        {
            _itemTypeList = GetItemIDs();
            _itemList = _itemTypeList.ToDictionary(item => item.Key, item => item.Key.Split(':')[1]);
            _colorSetList = LoadColorSetList();
            _itemModelInfo = new ItemModelInfo();
            ItemIDList = _itemTypeList.Select(item => item.Key).ToList();
            ItemNameList = _itemTypeList.Select(item => item.Value.Name).ToList();
            Model = new BlockModel();
            ItemID = ItemIDList.Count > 0 ? ItemIDList[0] : "";
            ItemName = ItemNameList.Count > 0 ? ItemNameList[0] : "";
            ModelEnabled = ItemNameList.Count > 0;
        }

        /// <summary>
        /// デフォルトコンストラクタ。空のデータで初期化します。
        /// </summary>
        public BlockModelLoarder() : base()
        {
            _itemTypeList = new Dictionary<string, ItemInfo>();
            _itemList = new Dictionary<string, string>();
            _colorSetList = LoadColorSetList();
            _itemModelInfo = new ItemModelInfo();
            ItemIDList = new List<string>();
            ItemNameList = new List<string>();
            Model = new BlockModel();
            ItemID = "";
            ItemName = "";
            ModelEnabled = false;
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
                _itemTypeList.Clear();
                _itemList.Clear();
                _colorSetList.Clear();
                ItemIDList.Clear();
                ItemNameList.Clear();
                Model.Dispose();

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
        ~BlockModelLoarder()
        {
            Dispose(false);
        }

        /// <summary>
        /// 指定されたアイテムIDに対応するブロックモデルをロードします。
        /// </summary>
        /// <param name="itemName">ロードするアイテムの名</param>
        public void LoadModel(string itemName)
        {
            Model?.Dispose();

            // アイテム名からアイテムIDを取得
            string itemID = _itemTypeList.FirstOrDefault(x => x.Value.Name == itemName).Key;

            // アイテムIDを取得
            if (!_itemTypeList.ContainsKey(itemID))
            {
                Model = new BlockModel();
                return;
            }

            string modelPath = "";
            if (CommonLib.CompareVersion(Version, "1.21.3") == 1)
            {
                // 1.21.4以降のバージョンでは、アイテムモデルのパスが変更されているため、特別な処理を行う
                _itemModelInfo?.Dispose();
                _itemModelInfo = new ItemModelInfo(itemID, JarPath, LanguageCode, Version);
                modelPath = _itemModelInfo.GetModelPath();
            }
            else
            {
                // 1.21.3以前のバージョンでは、アイテムモデルのパスはアイテムIDに基づいている
                modelPath = $"assets/{NameSpace}/models/item/{_itemList[itemID]}.json";
            }

            // モデルファイルをロード
            Model = new BlockModel(_itemTypeList[itemID].Type, itemID);
            LoadModelRecursive(modelPath);
            if (Model.ModelFilePath.Count > 0)
            {
                ItemID = itemID;
                ItemName = itemName;
                ModelEnabled = true;
            }

            // テクスチャをロード
            LoadTextures();
            return;
        }

        /// <summary>
        /// 指定されたパスからモデルデータを再帰的にロードします。
        /// </summary>
        /// <param name="modelPath">モデルファイルのパス</param>
        /// <returns>ロード結果</returns>
        private bool LoadModelRecursive(string modelPath)
        {
            // モデルファイルをロード
            var model = JsonLoad<Dictionary<string, object>>(modelPath);
            if (model.Count == 0)
            {
                return false;
            }

            Model.ModelFilePath.Add(modelPath);
            foreach (string key in model.Keys)
            {
                switch (key)
                {
                    // モデル追加
                    case "elements":
                        Model.AddElements(CommonLib.DeserializeJson<List<object>>(model["elements"].ToString()));
                        break;

                    // テクスチャ追加
                    case "textures":
                        Model.AddTextures(CommonLib.DeserializeJson<Dictionary<string, string>>(model["textures"].ToString()));
                        break;

                    // モデル表示設定追加
                    case "display":
                        Model.AddDisplays(CommonLib.DeserializeJson<Dictionary<string, object>>(model["display"].ToString()));
                        break;

                    // 親モデル読み込み
                    case "parent":
                        string? parent = model["parent"].ToString();
                        if (parent != null)
                        {
                            modelPath = $"assets/{NameSpace}/models/{parent.Replace($"{NameSpace}:", "")}.json";
                            LoadModelRecursive(modelPath);
                        }

                        break;
                }
            }

            // モデルの要素が空の場合、アイテムモデルとして扱う
            if (Model.Elements.Count == 0)
            {
                Model.ModelType = "item";
            }
            else
            {
                Model.ModelType = "block";
            }

            return true;
        }

        /// <summary>
        /// 指定されたBlockModelに対応するテクスチャをロードします。
        /// </summary>
        /// <param name="blockModel">テクスチャをロードする対象のBlockModel。</param>
        public void LoadTextures()
        {
            // テクスチャがない場合は何もしない
            Regex rx = new Regex(@"layer[0-9]{1}", RegexOptions.Compiled);
            foreach (var texture in Model.Textures)
            {
                if (texture.Value.StartsWith("#"))
                {
                    continue;
                }

                // テクスチャのパスを生成
                string textureName = texture.Value.Replace($"{NameSpace}:", "");
                string texturePath = $"assets/{NameSpace}/textures/{textureName}.png";

                // 乗算する色を取得
                string color = GetTextureColor(textureName);

                // レイヤー指定がある場合はレイヤー順にオーバーレイする
                string key = rx.IsMatch(texture.Key) ? "layer" : texture.Key;
                if (Model.TextureImages.ContainsKey(key))
                {
                    Mat<Vec4b> overlay = Model.TextureImages[key].Clone();
                    Mat<Vec4b> orgTexture = LoadImage(texturePath);

                    // カラーを乗算する
                    if (color != "")
                    {
                        ImageProcessing.ImageMultiply(color, ref orgTexture);
                    }

                    ImageProcessing.ImageOverlay(orgTexture, ref overlay);
                    Model.TextureImages[key] = overlay;
                }
                else
                {
                    Mat<Vec4b> orgTexture = LoadImage(texturePath);

                    // カラーを乗算する
                    if (color != "")
                    {
                        ImageProcessing.ImageMultiply(color, ref orgTexture);
                    }

                    // 新しいテクスチャを追加
                    Model.AddTexureImages(key, orgTexture);
                }
            }

            return;
        }

        /// <summary>
        /// アイテム名からアイテムIDを取得
        /// </summary>
        /// <param name="itemName">アイテム名</param>
        /// <returns>アイテムID</returns>
        public string GetItemID(string itemName)
        {
            // アイテム名からアイテムIDを取得
            var item = _itemTypeList.FirstOrDefault(x => x.Value.Name == itemName);
            if (item.Equals(default(KeyValuePair<string, ItemInfo>)))
            {
                return "";
            }

            return item.Key;
        }

        /// <summary>
        /// アイテムIDからアイテム名を取得
        /// </summary>
        /// <param name="itemID">アイテムID</param>
        /// <returns>アイテム名</returns>
        public string GetItemName(string itemID)
        {
            // アイテムIDからアイテム名を取得
            if (_itemTypeList.ContainsKey(itemID))
            {
                return _itemTypeList[itemID].Name;
            }

            return "";
        }

        /// <summary>
        /// 指定されたアイテムIDが存在するか確認する。
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public bool ExsistsItemID(string itemID)
        {
            // アイテムIDが存在するか確認
            return _itemTypeList.ContainsKey(itemID);
        }

        /// <summary>
        /// 指定されたアイテム名が存在するか確認する。
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public bool ExistsItemName(string itemName)
        {
            // アイテム名が存在するか確認
            var item = _itemTypeList.FirstOrDefault(x => x.Value.Name == itemName);
            if (item.Equals(default(KeyValuePair<string, ItemInfo>)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Jarファイル内の言語ファイルからアイテムIDを抽出します。
        /// </summary>
        /// <returns>アイテムIDとそのタイプのマッピング。</returns>
        private Dictionary<string, ItemInfo> GetItemIDs()
        {
            // 言語ファイルを読み込む
            Dictionary<string, string> lang = JsonLoad<Dictionary<string, string>>($"assets/{NameSpace}/lang/en_us.json");
            Dictionary<string, string> otherLang;
            if (LanguageCode != "en_us")
            {
                // 他の言語ファイルを読み込む
                if (NameSpace == "minecraft")
                {
                    otherLang = CommonLib.ReadJson<Dictionary<string, string>>(GenerateResourcePath());
                }
                else
                {
                    otherLang = JsonLoad<Dictionary<string, string>>($"assets/{NameSpace}/lang/{LanguageCode}.json");
                }
            }
            else
            {
                // 英語以外の言語ファイルがない場合は、英語の言語ファイルをコピー
                otherLang = new Dictionary<string, string>(lang);
            }

            // 言語ファイルからアイテムIDを抽出
            var filteredItems = lang.Keys.Where(x => x.Contains($"block.{NameSpace}") || x.Contains($"item.{NameSpace}")).ToList();

            // 1.21.4以降はitems配下のアイテムモデルでフィルタリングする
            Regex rx = new Regex($@"(block|item)\.{NameSpace}\.", RegexOptions.Compiled);
            if (CommonLib.CompareVersion(Version, "1.21.3") == 1)
            {
                var itemList = GetFileList($"assets/{NameSpace}/items");
                filteredItems = filteredItems.Where(x => itemList.Contains($"{rx.Replace(x, "")}.json")).ToList();
            }

            // アイテムリスト作成
            Dictionary<string, ItemInfo> itemTypeList = new Dictionary<string, ItemInfo>();
            foreach (string item in filteredItems)
            {
                // アイテムIDを抽出
                string itemID = rx.Replace(item, $"{NameSpace}:").Split('.')[0];
                if (itemTypeList.ContainsKey(itemID))
                {
                    continue;
                }

                // アイテムIDとそのタイプを追加
                itemTypeList.Add(itemID, new ItemInfo(item.Contains($"block.{NameSpace}") ? "block" : "item", otherLang[item]));
            }

            // アイテムIDをソートして返す
            return itemTypeList.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// カラーパレットセットをロードする。
        /// </summary>
        /// <returns>カラーパレットセットのリスト</returns>
        private Dictionary<string, Dictionary<string, object>> LoadColorSetList()
        {
            // カラーパレットセットを読み込む
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (Stream? stream = assembly.GetManifestResourceStream("MCModelRenderer.Resources.colorset.json"))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return CommonLib.DeserializeJson<Dictionary<string, Dictionary<string, object>>>(reader.ReadToEnd());
                    }
                }
            }

            return new Dictionary<string, Dictionary<string, object>>();
        }

        /// <summary>
        /// 指定されたテクスチャ名に対応する色を取得する。
        /// </summary>
        /// <param name="textureName"></param>
        /// <returns></returns>
        private string GetTextureColor(string textureName)
        {
            // 名前空間が存在しない場合は空文字列を返す
            if (!_colorSetList.ContainsKey(NameSpace))
            {
                return "";
            }

            // カラーパレットセットからテクスチャの色を取得
            var colorSet = _colorSetList[NameSpace].FirstOrDefault(x => x.Key == textureName);
            if (colorSet.Equals(default(KeyValuePair<string, object>)))
            {
                return "";
            }

            // 色の値が文字列型であることを確認
            string color = colorSet.Value.ToString() ?? "";
            if (color.Length == 6)
            {
                return color;
            }

            // 色の値がJSON形式である場合、デシリアライズして取得
            string itemName = ItemID.Replace($"{NameSpace}:", "");
            var spawnEggColor = CommonLib.DeserializeJson<Dictionary<string, string>>(color);
            var spawnEggColorSet = spawnEggColor.FirstOrDefault(x => x.Key == itemName);
            if (colorSet.Equals(default(KeyValuePair<string, string>)))
            {
                return "";
            }

            return spawnEggColorSet.Value;
        }
    }
}
