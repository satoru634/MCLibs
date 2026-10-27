using MCToolsCommonLib.Common;
using OpenCvSharp;

namespace MCModelRenderer.MCModels
{
    /// <summary>
    /// Minecraftのブロックモデルを表すクラス。
    /// </summary>
    public class BlockModel : IDisposable
    {
        /// <summary>
        /// モデルの種類。
        /// </summary>
        public string ModelType { get; set; }

        /// <summary>
        /// モデルの要素を格納するリスト。
        /// </summary>
        public List<ModelElements> Elements { get; set; }

        /// <summary>
        /// モデルのテクスチャを格納する辞書。
        /// </summary>
        public Dictionary<string, string> Textures { get; set; }

        /// <summary>
        /// モデルのテクスチャ画像を格納する辞書。
        /// </summary>
        public Dictionary<string, Mat<Vec4b>> TextureImages { get; set; }

        /// <summary>
        /// モデルの表示に関する情報を格納する辞書。
        /// </summary>
        public Dictionary<string, ModelDisplay> Displays { get; set; }

        /// <summary>
        /// モデルファイルのパスを格納するリスト。
        /// </summary>
        public List<string> ModelFilePath { get; set; }

        /// <summary>
        /// アイテムID。
        /// </summary>
        public string ItemID { get; set; }

        /// <summary>
        /// リソースの解放状態を示すフラグ。
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="modelType">モデルの種類</param>
        /// <param name="itemID">アイテムID</param>
        public BlockModel(string modelType, string itemID)
        {
            ModelType = modelType;
            Elements = new List<ModelElements>();
            Textures = new Dictionary<string, string>();
            TextureImages = new Dictionary<string, Mat<Vec4b>>();
            Displays = new Dictionary<string, ModelDisplay>();
            ModelFilePath = new List<string>();
            ItemID = itemID;
        }

        /// <summary>
        /// デフォルトコンストラクタ。
        /// </summary>
        public BlockModel()
        {
            ModelType = "";
            Elements = new List<ModelElements>();
            Textures = new Dictionary<string, string>();
            TextureImages = new Dictionary<string, Mat<Vec4b>>();
            Displays = new Dictionary<string, ModelDisplay>();
            ModelFilePath = new List<string>();
            ItemID = "";
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

                if (Elements.Count > 0)
                {
                    foreach (var element in Elements)
                    {
                        element.Dispose();
                    }
                }

                if (TextureImages.Count > 0)
                {
                    foreach (var texture in TextureImages)
                    {
                        texture.Value.Dispose();
                    }
                }

                Elements.Clear();
                Textures.Clear();
                TextureImages.Clear();
                Displays.Clear();
                ModelFilePath.Clear();

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
        ~BlockModel()
        {
            Dispose(false);
        }

        /// <summary>
        /// 要素を追加する。
        /// </summary>
        /// <param name="elements">要素のリスト</param>
        public void AddElements(List<object> elements)
        {
            foreach (object orgElem in elements)
            {
                string? strElem = orgElem.ToString();
                if (strElem == null)
                {
                    continue;
                }

                var element = CommonLib.DeserializeJson<Dictionary<string, object>>(strElem);
                ModelElements newElement = new ModelElements();
                foreach (string key in element.Keys)
                {
                    switch (key)
                    {
                        // 要素の開始位置と終了位置を設定する。
                        case "from":
                        case "to":
                            newElement.AddPos(element[key], key);
                            break;

                        // 要素の回転情報を設定する。
                        case "rotation":
                            newElement.AddRotation(element[key]);
                            break;

                        // 要素の面情報を設定する。
                        case "faces":
                            newElement.AddFaces(element[key]);
                            break;
                    }
                }

                Elements.Add(newElement);
            }

            return;
        }

        /// <summary>
        /// テクスチャを追加する。
        /// </summary>
        /// <param name="textures">テクスチャ情報</param>
        public void AddTextures(Dictionary<string, string> textures)
        {
            foreach (var texture in textures)
            {
                if (!Textures.ContainsKey(texture.Key))
                {
                    Textures.Add(texture.Key, texture.Value);
                }
                else
                {
                    Textures[texture.Key] = texture.Value;
                }
            }

            return;
        }

        /// <summary>
        /// テクスチャ画像を追加する。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="img">画像</param>
        public void AddTexureImages(string key, Mat<Vec4b> img)
        {
            TextureImages.Add(key, img);
            return;
        }

        /// <summary>
        /// モデルの表示に関する情報を追加する。
        /// </summary>
        /// <param name="displays"></param>
        public void AddDisplays(Dictionary<string, object> displays)
        {
            foreach (var display in displays)
            {
                if (!Displays.ContainsKey(display.Key))
                {
                    Displays.Add(display.Key, new ModelDisplay(display.Value));
                }
                else
                {
                    Displays[display.Key] = new ModelDisplay(display.Value);
                }
            }

            return;
        }
    }
}
