using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.BaseData
{
    /// <summary>
    /// 2D座標を表すクラス
    /// </summary>
    public class PointXZ
    {
        /// <summary>
        /// X座標
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Z座標
        /// </summary>
        public int Z { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public PointXZ()
        {
            X = 0;
            Z = 0;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="z">Z座標</param>
        public PointXZ(int x = 0, int z = 0)
        {
            X = x;
            Z = z;
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="point">2D座標</param>
        public PointXZ(PointXZ point)
        {
            X = point.X;
            Z = point.Z;
        }

        /// <summary>
        /// 2D座標の最大値を更新
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="z">Z座標</param>
        public void Max(int x, int z)
        {
            X = Math.Max(x, X);
            Z = Math.Max(z, Z);
        }

        /// <summary>
        /// 2D座標の最小値を更新
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="z">Z座標</param>
        public void Min(int x, int z)
        {
            X = Math.Min(x, X);
            Z = Math.Min(z, Z);
        }

        /// <summary>
        /// 2D座標を指定された値だけシフトする
        /// </summary>
        /// <param name="x">X座標の移動量</param>
        /// <param name="z">Z座標の移動量</param>
        public void Shift(int x, int z)
        {
            X += x;
            Z += z;
        }

        /// <summary>
        /// すべての座標を指定された値だけシフトする
        /// </summary>
        /// <param name="shift">移動量</param>
        public void ShiftAll(int shift)
        {
            Shift(shift, shift);
            return;
        }

        /// <summary>
        /// 座標を文字列として表現する
        /// </summary>
        /// <returns>文字列</returns>
        public override string ToString()
        {
            return $"X: {X}, Z:{Z}";
        }
    }
}
