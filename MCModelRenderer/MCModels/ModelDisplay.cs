using MCToolsCommonLib.Common;
using System.Windows.Media.Media3D;

namespace MCModelRenderer.MCModels
{
    /// <summary>
    /// モデルの表示に関する情報を保持するクラス。
    /// </summary>
    public class ModelDisplay
    {
        /// <summary>
        /// モデルの回転情報。
        /// </summary>
        public Point3D Rotation { get; set; }

        /// <summary>
        /// モデルの位置情報。
        /// </summary>
        public Point3D Translation { get; set; }

        /// <summary>
        /// モデルのスケール情報。
        /// </summary>
        public Point3D Scale { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ。
        /// </summary>
        public ModelDisplay()
        {
            Rotation = new Point3D();
            Translation = new Point3D();
            Scale = new Point3D();
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="orgDisplay">オリジナルのDisplayデータ(JSON形式)</param>
        public ModelDisplay(object orgDisplay)
        {
            Rotation = new Point3D();
            Translation = new Point3D();
            Scale = new Point3D();

            string? strDisplay = orgDisplay.ToString();
            if (strDisplay == null)
            {
                return;
            }

            var display = CommonLib.DeserializeJson<Dictionary<string, List<double>>>(strDisplay);
            foreach (var pair in display)
            {
                switch (pair.Key)
                {
                    // 回転情報の設定
                    case "rotation":
                        Rotation = new Point3D(pair.Value[0], pair.Value[1], pair.Value[2]);
                        break;

                    // 位置情報の設定
                    case "translation":
                        Translation = new Point3D(pair.Value[0], pair.Value[1], pair.Value[2]);
                        break;

                    // スケール情報の設定
                    case "scale":
                        Scale = new Point3D(pair.Value[0], pair.Value[1], pair.Value[2]);
                        break;
                }
            }

            return;
        }
    }
}
