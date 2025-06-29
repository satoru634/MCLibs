using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.BaseData
{
    /// <summary>
    /// 3D座標を表すクラス
    /// </summary>
    public class Point3D
    {
        /// <summary>
        /// X座標
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y座標
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Z座標
        /// </summary>
        public int Z { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public Point3D()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="z">Z座標</param>
        public Point3D(int x = 0, int y = 0, int z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="point">3D座標</param>
        public Point3D(Point3D point)
        {
            X = point.X;
            Y = point.Y;
            Z = point.Z;
        }

        /// <summary>
        /// 座標の最大値を更新
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="z">Z座標</param>
        public void Max(int x, int y, int z)
        {
            X = Math.Max(x, X);
            Y = Math.Max(y, Y);
            Z = Math.Max(z, Z);
        }

        /// <summary>
        /// 座標の最小値を更新
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="z">Z座標</param>
        public void Min(int x, int y, int z)
        {
            X = Math.Min(x, X);
            Y = Math.Min(y, Y);
            Z = Math.Min(z, Z);
        }

        /// <summary>
        /// 座標を指定された値だけシフトする
        /// </summary>
        /// <param name="x">X座標の移動量</param>
        /// <param name="y">Y座標の移動量</param>
        /// <param name="z">Z座標の移動量</param>
        public void Shift(int x, int y, int z)
        {
            X += x;
            Y += y;
            Z += z;
        }

        /// <summary>
        /// すべての座標を指定された値だけシフトする
        /// </summary>
        /// <param name="shift">移動量</param>
        public void ShiftAll(int shift)
        {
            Shift(shift, shift, shift);
            return;
        }

        /// <summary>
        /// 座標を文字列として表現する
        /// </summary>
        /// <returns>文字列</returns>
        public override string ToString()
        {
            return $"X: {X}, Y: {Y}, Z:{Z}";
        }
    }
}
