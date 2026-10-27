using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.BaseData
{
    public class PointXZ
    {
        public int X { get; set; }
        public int Z { get; set; }

        public PointXZ()
        {
            X = 0;
            Z = 0;
        }

        public PointXZ(int x = 0, int z = 0)
        {
            X = x;
            Z = z;
        }

        public PointXZ(PointXZ point)
        {
            X = point.X;
            Z = point.Z;
        }

        public void Max(int x, int z)
        {
            X = Math.Max(x, X);
            Z = Math.Max(z, Z);
        }

        public void Min(int x, int z)
        {
            X = Math.Min(x, X);
            Z = Math.Min(z, Z);
        }

        public void Shift(int x, int z)
        {
            X += x;
            Z += z;
        }

        public void ShiftAll(int shift)
        {
            Shift(shift, shift);
            return;
        }

        public override string ToString()
        {
            return $"X: {X}, Z:{Z}";
        }
    }
}
