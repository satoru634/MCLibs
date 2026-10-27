using MCToolsCommonLib.Common;
using MCToolsCommonLib.Utils;
using System.Windows.Media.Media3D;

namespace MCModelRenderer.MCModels
{
    /// <summary>
    /// モデルの回転を表すクラス。
    /// </summary>
    public class ModelRotation
    {
        /// <summary>
        /// 回転角度（度単位）。
        /// </summary>
        public double Angle { get; set; }

        /// <summary>
        /// 回転軸の名前（"x"、"y"、"z"など）。
        /// </summary>
        public string Axis { get; set; }

        /// <summary>
        /// 回転の原点座標。
        /// </summary>
        public Point3D Origin { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ。
        /// </summary>
        public ModelRotation()
        {
            Angle = 0.0f;
            Axis = "";
            Origin = new Point3D();
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="rotation">オリジナルの回転データ</param>
        public ModelRotation(Dictionary<string, object> rotation)
        {
            Angle = 0.0f;
            Axis = "";
            Origin = new Point3D();

            foreach (var pair in rotation)
            {
                switch (pair.Key)
                {
                    // 回転角度を設定
                    case "angle":
                        Angle = ConvertJsonValue.ConvertDouble(pair.Value);
                        break;

                    // 回転軸を設定
                    case "axis":
                        Axis = ConvertJsonValue.ConvertStr(pair.Value);
                        break;

                    // 回転の原点座標を設定
                    case "origin":
                        Origin = InitOrigin(pair.Value);
                        break;
                }
            }
        }

        /// <summary>
        /// 回転の原点座標を初期化する。
        /// </summary>
        /// <param name="orgOrigin">原点座標(JSON形式)</param>
        /// <returns>原点座標</returns>
        private Point3D InitOrigin(object orgOrigin)
        {
            string? strOrigin = orgOrigin.ToString();
            if (strOrigin == null)
            {
                return new Point3D();
            }

            var rawOrigin = CommonLib.DeserializeJson<List<double>>(strOrigin);
            if (rawOrigin.Count != 3)
            {
                return new Point3D();
            }

            return new Point3D(rawOrigin[0], rawOrigin[1], rawOrigin[2]);
        }
    }
}
