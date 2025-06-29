using MCToolsCommonLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.BaseData
{
    /// <summary>
    /// 2D座標と範囲を表すクラス
    /// </summary>
    public class PointRange
    {
        /// <summary>
        /// 2D座標を表すPointXZオブジェクト
        /// </summary>
        public PointXZ Point { get; set; }

        /// <summary>
        /// 範囲を表す整数値
        /// </summary>
        public int Range { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="z">Z座標</param>
        /// <param name="range">範囲</param>
        public PointRange(int x = 0, int z = 0, int range = int.MaxValue)
        {
            Point = new PointXZ(x, z);
            Range = range;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="point">2D座標</param>
        /// <param name="range">範囲</param>
        public PointRange(PointXZ point, int range = int.MaxValue)
        {
            Point = point;
            Range = range;
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="pointRange">2D座標と範囲</param>
        public PointRange(PointRange pointRange)
        {
            Point = new PointXZ(pointRange.Point);
            Range = pointRange.Range;
        }

        /// <summary>
        /// 当たり判定(矩形)
        /// </summary>
        /// <param name="target">対象の2D座標と範囲</param>
        /// <returns>当たり判定の結果</returns>
        public bool IsCollisionWithRect(PointRange target)
        {
            PointXZ baseLT = new PointXZ(Point);
            PointXZ baseRB = new PointXZ(Point);
            baseLT.ShiftAll(-Range);
            baseRB.ShiftAll(Range);

            PointXZ targetLT = new PointXZ(target.Point);
            PointXZ targetRB = new PointXZ(target.Point);
            targetLT.ShiftAll(target.Range);
            targetRB.ShiftAll(target.Range);
            return baseLT.X <= targetRB.X && targetLT.X <= baseRB.X && baseLT.Z <= targetRB.Z && targetLT.Z <= baseRB.Z;
        }

        /// <summary>
        /// 当たり判定(矩形)
        /// </summary>
        /// <param name="targetLT">左上の3D座標</param>
        /// <param name="targetRB">右下の3D座標</param>
        /// <returns>当たり判定の結果</returns>
        public bool IsCollisionWithRect(Point3D targetLT, Point3D targetRB)
        {
            PointXZ baseLT = new PointXZ(Point);
            PointXZ baseRB = new PointXZ(Point);
            baseLT.ShiftAll(-Range);
            baseRB.ShiftAll(Range);
            return baseLT.X <= targetRB.X && targetLT.X <= baseRB.X && baseLT.Z <= targetRB.Z && targetLT.Z <= baseRB.Z;
        }

        /// <summary>
        /// 当たり判定(円形)
        /// </summary>
        /// <param name="target">対象の2D座標と範囲</param>
        /// <returns>当たり判定の結果</returns>
        public bool IsCollisionWithCircle(PointRange target)
        {
            double dist = CommonLib.CalcDistance2D(Point, target.Point);
            return dist <= Range;
        }

        /// <summary>
        /// 当たり判定(円形)
        /// </summary>
        /// <param name="targetLT">左上の3D座標</param>
        /// <param name="targetRB">右下の3D座標</param>
        /// <returns>当たり判定の結果</returns>
        public bool IsCollisionWithCircle(Point3D targetLT, Point3D targetRB)
        {
            double distLT = CommonLib.CalcDistance2D(Point, targetLT);
            double distRB = CommonLib.CalcDistance2D(Point, targetRB);
            return distLT <= Range || distRB <= Range;
        }

        /// <summary>
        /// 座標を文字列として表現する
        /// </summary>
        /// <returns>文字列</returns>
        public override string ToString()
        {
            return $"X: {Point.X}, Z:{Point.Z}, Range:{Range}";
        }
    }
}
