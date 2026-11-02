using System.Windows.Media.Media3D;
using MCModelRenderer.Utils;
using OpenCvSharp;
using SharpDX;
using Matrix = SharpDX.Matrix;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

namespace MCLibsTests.MCModelRenderer.Utils
{
    /// <summary>
    /// CommonCalc のテスト
    /// </summary>
    public class CommonCalcTests
    {
        private const int Precision = 5;

        // ── CalcFov ───────────────────────────────────────────────────────────

        [Theory]
        [InlineData(1.0f, 1.0f, 45.0f)]
        [InlineData(0.0f, 1.0f, 0.0f)]
        public void CalcFov_ReturnsAngleInDegrees(float width, float distance, float expected)
        {
            Assert.Equal(expected, CommonCalc.CalcFov(width, distance), Precision);
        }

        // ── TransformPos ──────────────────────────────────────────────────────

        [Fact]
        public void TransformPos_ListAndList_ReturnsMovedAndScaledPoint()
        {
            var result = CommonCalc.TransformPos([0.0, 8.0, 16.0], -8.0, [0.5, 1.0, 2.0]);

            Assert.Equal(new Point3D(-4.0, 0.0, 16.0), result);
        }

        [Fact]
        public void TransformPos_Point3DAndPoint3D_ReturnsMovedAndScaledVector()
        {
            var result = CommonCalc.TransformPos(new Point3D(0, 8, 16), -8.0, new Point3D(0.625, 0.625, 0.625));

            Assert.Equal(new Vector3D(-5.0, 0.0, 5.0), result);
        }

        [Fact]
        public void TransformPos_Vector3_ReturnsMovedAndScaledVector()
        {
            var result = CommonCalc.TransformPos(new Vector3(0, 8, 16), -8.0f, new Vector3(1, 2, 3));

            Assert.Equal(new Vector3(-8, 0, 24), result);
        }

        [Fact]
        public void TransformPos_Point3DAndList_ReturnsMovedAndScaledVector()
        {
            var result = CommonCalc.TransformPos(new Point3D(16, 16, 16), -8.0, [1.0, 0.5, 0.25]);

            Assert.Equal(new Vector3D(8.0, 4.0, 2.0), result);
        }

        // ── CalculateRotationMatrix（SharpDX） ────────────────────────────────

        [Fact]
        public void CalculateRotationMatrix_Float_Zero_ReturnsIdentity()
        {
            Assert.Equal(Matrix.Identity, CommonCalc.CalculateRotationMatrix(0f, 0f, 0f));
        }

        [Fact]
        public void CalculateRotationMatrix_Float_X90_RotatesYToZ()
        {
            var matrix = CommonCalc.CalculateRotationMatrix(90f, 0f, 0f);

            var result = Vector3.TransformCoordinate(Vector3.UnitY, matrix);

            AssertVector(new Vector3(0, 0, 1), result);
        }

        [Fact]
        public void CalculateRotationMatrix_Vector3_SameAsFloatOverload()
        {
            var expected = CommonCalc.CalculateRotationMatrix(30f, 135f, 10f);

            var actual = CommonCalc.CalculateRotationMatrix(new Vector3(30f, 135f, 10f));

            Assert.Equal(expected, actual);
        }

        // ── CalculateRotationMatrix（WPF） ────────────────────────────────────

        [Fact]
        public void CalculateRotationMatrix_Double_Zero_ReturnsIdentity()
        {
            Assert.True(CommonCalc.CalculateRotationMatrix(0.0, 0.0, 0.0).IsIdentity);
        }

        [Fact]
        public void CalculateRotationMatrix_Double_X90_RotatesYToZ()
        {
            var matrix = CommonCalc.CalculateRotationMatrix(90.0, 0.0, 0.0);

            var result = matrix.Transform(new Vector3D(0, 1, 0));

            AssertVector(new Vector3D(0, 0, 1), result);
        }

        [Fact]
        public void CalculateRotationMatrix_Double_Y90_RotatesZToX()
        {
            var matrix = CommonCalc.CalculateRotationMatrix(0.0, 90.0, 0.0);

            var result = matrix.Transform(new Vector3D(0, 0, 1));

            AssertVector(new Vector3D(1, 0, 0), result);
        }

        [Fact]
        public void CalculateRotationMatrix_Point3D_SameAsDoubleOverload()
        {
            var expected = CommonCalc.CalculateRotationMatrix(30.0, 135.0, 0.0);

            var actual = CommonCalc.CalculateRotationMatrix(new Point3D(30, 135, 0));

            Assert.Equal(expected, actual);
        }

        // ── ConvertMatVec4bToColor4Array ──────────────────────────────────────

        [Fact]
        public void ConvertMatVec4bToColor4Array_ConvertsBGRAToRGBAInRowMajorOrder()
        {
            using var mat = new Mat<Vec4b>(2, 2);
            var indexer = mat.GetIndexer();
            indexer[0, 0] = new Vec4b(255, 0, 0, 255);  // 青
            indexer[0, 1] = new Vec4b(0, 255, 0, 255);  // 緑
            indexer[1, 0] = new Vec4b(0, 0, 255, 255);  // 赤
            indexer[1, 1] = new Vec4b(0, 0, 0, 0);      // 透明

            var colors = CommonCalc.ConvertMatVec4bToColor4Array(mat);

            Assert.Equal(
                [
                    new Color4(0, 0, 1, 1),
                    new Color4(0, 1, 0, 1),
                    new Color4(1, 0, 0, 1),
                    new Color4(0, 0, 0, 0),
                ],
                colors);
        }

        [Fact]
        public void ConvertMatVec4bToColor4Array_Null_ReturnsEmpty()
        {
            Assert.Empty(CommonCalc.ConvertMatVec4bToColor4Array(null!));
        }

        /// <summary>
        /// ベクトルが許容誤差内で等しいことを検証する。
        /// </summary>
        private static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.Equal(expected.X, actual.X, Precision);
            Assert.Equal(expected.Y, actual.Y, Precision);
            Assert.Equal(expected.Z, actual.Z, Precision);
        }

        /// <summary>
        /// ベクトルが許容誤差内で等しいことを検証する。
        /// </summary>
        private static void AssertVector(Vector3D expected, Vector3D actual)
        {
            Assert.Equal(expected.X, actual.X, Precision);
            Assert.Equal(expected.Y, actual.Y, Precision);
            Assert.Equal(expected.Z, actual.Z, Precision);
        }
    }
}
