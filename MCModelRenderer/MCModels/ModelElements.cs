using MCToolsCommonLib.Common;
using System.Windows.Media.Media3D;

namespace MCModelRenderer.MCModels
{
    /// <summary>
    /// ModelElementsクラスは、Minecraftモデルの要素を表す。
    /// </summary>
    public class ModelElements : IDisposable
    {
        /// <summary>
        /// 要素の開始位置。
        /// </summary>
        public Point3D From { get; set; }

        /// <summary>
        /// 要素の終了位置。
        /// </summary>
        public Point3D To { get; set; }

        /// <summary>
        /// 要素の回転情報。
        /// </summary>
        public ModelRotation Rotate { get; set; }

        /// <summary>
        /// 要素の面情報を格納する辞書。
        /// </summary>
        public Dictionary<string, ModelFace> Faces { get; set; }

        /// <summary>
        /// 要素の移動量。
        /// </summary>
        public float MoveAmount { get; set; }

        /// <summary>
        /// リソースの解放状態を示すフラグ。
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// デフォルトコンストラクタ。
        /// </summary>
        public ModelElements()
        {
            From = new Point3D();
            To = new Point3D();
            Rotate = new ModelRotation();
            Faces = new Dictionary<string, ModelFace>();
            MoveAmount = -8.0f;
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="from">要素の開始位置</param>
        /// <param name="to">要素の終了位置</param>
        /// <param name="rotate">要素の回転情報</param>
        /// <param name="faces">要素の面情報</param>
        /// <param name="moveAmount">要素の移動量</param>
        public ModelElements(Point3D from, Point3D to, ModelRotation rotate, Dictionary<string, ModelFace> faces, float moveAmount)
        {
            From = from;
            To = to;
            Rotate = rotate;
            Faces = faces;
            MoveAmount = moveAmount;
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

                if (Faces.Count > 0)
                {
                    foreach (var face in Faces)
                    {
                        face.Value.Dispose();
                    }
                }

                Faces.Clear();

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
        ~ModelElements()
        {
            Dispose(false);
        }

        /// <summary>
        /// 要素の位置情報を追加する。
        /// </summary>
        /// <param name="orgPos"></param>
        /// <param name="key"></param>
        public void AddPos(object orgPos, string key)
        {
            string? pos = orgPos.ToString();
            if (pos == null)
            {
                return;
            }

            switch (key)
            {
                // 要素の開始位置を設定する。
                case "from":
                    var from = CommonLib.DeserializeJson<List<double>>(pos);
                    if (from.Count == 3)
                    {
                        From = new Point3D(from[0], from[1], from[2]);
                    }

                    break;

                // 要素の終了位置を設定する。
                case "to":
                    var to = CommonLib.DeserializeJson<List<double>>(pos);
                    if (to.Count == 3)
                    {
                        To = new Point3D(to[0], to[1], to[2]);
                    }

                    break;
            }

            return;
        }

        /// <summary>
        /// 要素の回転情報を追加する。
        /// </summary>
        /// <param name="orgRotation"></param>
        public void AddRotation(object orgRotation)
        {
            string? strRotation = orgRotation.ToString();
            if (strRotation == null)
            {
                return;
            }

            Rotate = new ModelRotation(CommonLib.DeserializeJson<Dictionary<string, object>>(strRotation));
            return;
        }

        /// <summary>
        /// 要素の面情報を追加する。
        /// </summary>
        /// <param name="orgFaces"></param>
        public void AddFaces(object orgFaces)
        {
            string? strFaces = orgFaces.ToString();
            if (strFaces == null)
            {
                return;
            }

            var faces = CommonLib.DeserializeJson<Dictionary<string, object>>(strFaces);
            foreach (var face in faces)
            {
                Faces.Add(face.Key, new ModelFace(face.Value));
            }

            return;
        }
    }
}
