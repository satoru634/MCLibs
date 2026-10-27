using MCToolsCommonLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.BaseData
{
    public class PointRange
    {
        public PointXZ Point { get; set; }
        public int Range { get; set; }

        public PointRange(int x = 0, int z = 0, int range = int.MaxValue)
        {
            Point = new PointXZ(x, z);
            Range = range;
        }

        public PointRange(PointXZ point, int range = int.MaxValue)
        {
            Point = point;
            Range = range;
        }

        // 当たり判定(矩形)
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

        // 当たり判定(矩形)
        public bool IsCollisionWithRect(Point3D targetLT, Point3D targetRB)
        {
            PointXZ baseLT = new PointXZ(Point);
            PointXZ baseRB = new PointXZ(Point);
            baseLT.ShiftAll(-Range);
            baseRB.ShiftAll(Range);
            return baseLT.X <= targetRB.X && targetLT.X <= baseRB.X && baseLT.Z <= targetRB.Z && targetLT.Z <= baseRB.Z;
        }

        // 当たり判定(円形)
        public bool IsCollisionWithCircle(PointRange target)
        {
            double dist = CommonLib.CalcDistance2D(Point, target.Point);
            return dist <= Range;
        }

        // 当たり判定(円形)
        public bool IsCollisionWithCircle(Point3D targetLT, Point3D targetRB)
        {
            double distLT = CommonLib.CalcDistance2D(Point, targetLT);
            double distRB = CommonLib.CalcDistance2D(Point, targetRB);
            return distLT <= Range || distRB <= Range;
        }

        public override string ToString()
        {
            return $"X: {Point.X}, Z:{Point.Z}, Range:{Range}";
        }
    }
}
