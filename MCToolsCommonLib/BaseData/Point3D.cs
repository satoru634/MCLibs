using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.BaseData
{
    public class Point3D
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public Point3D()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Point3D(int x = 0, int y = 0, int z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void Max(int x, int y, int z)
        {
            X = Math.Max(x, X);
            Y = Math.Max(y, Y);
            Z = Math.Max(z, Z);
        }

        public void Min(int x, int y, int z)
        {
            X = Math.Min(x, X);
            Y = Math.Min(y, Y);
            Z = Math.Min(z, Z);
        }

        public void Shift(int x, int y, int z)
        {
            X += x;
            Y += y;
            Z += z;
        }

        public void ShiftAll(int shift)
        {
            Shift(shift, shift, shift);
            return;
        }

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}, Z:{Z}";
        }
    }
}
