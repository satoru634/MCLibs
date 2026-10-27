using OpenCvSharp;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using WpfQuaternion = System.Windows.Media.Media3D.Quaternion;

namespace MCModelRenderer.Utils
{
    public class CommonCalc
    {
        /// <summary>
        /// 視野角を算出する。
        /// </summary>
        /// <param name="width">幅</param>
        /// <param name="distance">距離</param>
        /// <returns>視野角</returns>
        static public float CalcFov(float width, float distance)
        {
            float angle = (float)Math.Atan(width / distance);
            return MathUtil.RadiansToDegrees(angle);
        }

        /// <summary>Add commentMore actions
        /// 座標を変換する。
        /// </summary>
        /// <param name="pos">元の座標</param>
        /// <param name="move">移動量</param>
        /// <param name="scale">スケール</param>
        /// <returns>変換されたPoint3D</returns>
        static public Point3D TransformPos(List<double> pos, double move, List<double> scale)
        {
            Point3D newPos = new Point3D();
            newPos.X = (pos[0] + move) * scale[0];
            newPos.Y = (pos[1] + move) * scale[1];
            newPos.Z = (pos[2] + move) * scale[2];
            return newPos;
        }

        /// <summary>Add commentMore actions
        /// 座標を変換する。
        /// </summary>
        /// <param name="pos">元の座標</param>
        /// <param name="move">移動量</param>
        /// <param name="scale">スケール</param>
        /// <returns>変換されたPoint3D</returns>
        static public Vector3D TransformPos(Point3D pos, double move, Point3D scale)
        {
            Vector3D newPos = new Vector3D();
            newPos.X = (pos.X + move) * scale.X;
            newPos.Y = (pos.Y + move) * scale.Y;
            newPos.Z = (pos.Z + move) * scale.Z;
            return newPos;
        }

        /// <summary>
        /// 座標を変換する。
        /// </summary>
        /// <param name="pos">元の座標</param>
        /// <param name="move">移動量</param>
        /// <param name="scale">スケール</param>
        /// <returns>変換後の座標</returns>
        static public Vector3 TransformPos(Vector3 pos, float move, Vector3 scale)
        {
            Vector3 newPos = new Vector3();
            newPos.X = (pos.X + move) * scale.X;
            newPos.Y = (pos.Y + move) * scale.Y;
            newPos.Z = (pos.Z + move) * scale.Z;
            return newPos;
        }

        /// <summary>Add commentMore actions
        /// 座標を変換する。
        /// </summary>
        /// <param name="pos">元の座標</param>
        /// <param name="move">移動量</param>
        /// <param name="scale">スケール</param>
        /// <returns>変換されたVector3D</returns>
        static public Vector3D TransformPos(Point3D pos, double move, List<double> scale)
        {
            Vector3D newPos = new Vector3D();
            newPos.X = (pos.X + move) * scale[0];
            newPos.Y = (pos.Y + move) * scale[1];
            newPos.Z = (pos.Z + move) * scale[2];
            return newPos;
        }

        /// <summary>
        /// 回転行列を計算する。
        /// </summary>
        /// <param name="rotation">回転角度</param>
        /// <returns>計算されたMatrix</returns>
        static public Matrix CalculateRotationMatrix(Vector3 rotation)
        {
            return CalculateRotationMatrix(rotation.X, rotation.Y, rotation.Z);
        }

        /// <summary>
        /// 回転行列を計算する。
        /// </summary>
        /// <param name="x">X軸の回転角度</param>
        /// <param name="y">Y軸の回転角度</param>
        /// <param name="z">Z軸の回転角度</param>
        /// <returns>計算後の回転行列</returns>
        static public Matrix CalculateRotationMatrix(float x, float y, float z)
        {
            Matrix matrix = Matrix.RotationX(MathUtil.DegreesToRadians(x));
            matrix = Matrix.RotationY(MathUtil.DegreesToRadians(y)) * matrix;
            matrix = Matrix.RotationZ(MathUtil.DegreesToRadians(z)) * matrix;
            return matrix;
        }

        /// <summary>Add commentMore actions
        /// 回転行列を計算する。
        /// </summary>
        /// <param name="rotation">回転角度</param>
        /// <returns>計算されたMatrix3D</returns>
        static public Matrix3D CalculateRotationMatrix(Point3D rotation)
        {
            return CalculateRotationMatrix(rotation.X, rotation.Y, rotation.Z);
        }

        /// <summary>Add commentMore actions
        /// 回転行列を計算する。
        /// </summary>
        /// <param name="x">X軸の回転角度</param>
        /// <param name="y">Y軸の回転角度</param>
        /// <param name="z">Z軸の回転角度</param>
        /// <returns>計算されたMatrix3D</returns>
        static public Matrix3D CalculateRotationMatrix(double x, double y, double z)
        {
            Matrix3D matrix = new Matrix3D();

            matrix.Rotate(new WpfQuaternion(new Vector3D(1, 0, 0), x));
            matrix.Rotate(new WpfQuaternion(new Vector3D(0, 1, 0) * matrix, y));
            matrix.Rotate(new WpfQuaternion(new Vector3D(0, 0, 1) * matrix, z));
            return matrix;
        }

        /// <summary>
        /// OpenCVSharpのMat<Vec4b> (BGRA) を SharpDXのColor4[] (RGBA) に変換します。
        /// </summary>
        /// <param name="mat">変換元のMat<Vec4b>オブジェクト。</param>
        /// <returns>変換後のColor4配列。</returns>
        static public Color4[] ConvertMatVec4bToColor4Array(Mat<Vec4b> mat)
        {
            if (mat == null)
            {
                return System.Array.Empty<Color4>();
            }

            int width = mat.Width;
            int height = mat.Height;
            Color4[] colorArray = new Color4[width * height];
            var indexer = mat.GetIndexer();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vec4b bgraPixel = indexer[y, x]; // OpenCVのVec4bはBGRAの順です

                    // BGRA (byte) から RGBA (float) へ変換し、値を0.0f～1.0fの範囲に正規化します
                    float r = bgraPixel.Item2 / 255f;
                    float g = bgraPixel.Item1 / 255f;
                    float b = bgraPixel.Item0 / 255f;
                    float a = bgraPixel.Item3 / 255f;

                    colorArray[y * width + x] = new Color4(r, g, b, a);
                }
            }
            return colorArray;
        }
    }
}
