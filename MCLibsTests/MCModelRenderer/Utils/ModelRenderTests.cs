using System.IO;
using MCLibsTests.Helpers;
using MCModelRenderer.MCModels;
using MCModelRenderer.Utils;
using OpenCvSharp;

namespace MCLibsTests.MCModelRenderer.Utils
{
    /// <summary>
    /// ModelRender のテスト
    /// </summary>
    /// <remarks>
    /// block モデルの描画・出力は Viewport3DX（WPF の STA スレッドと GPU）が必要なため対象外とし、
    /// ビューポートを使わずにテクスチャを直接書き出す item モデルの出力を検証する。
    /// </remarks>
    public class ModelRenderTests : IDisposable
    {
        private readonly TempDirectory _temp = new TempDirectory();

        public void Dispose()
        {
            _temp.Dispose();
        }

        /// <summary>
        /// 左上の1画素だけが赤で、それ以外が青の 16x16 テクスチャを持つ item モデルを作成する。
        /// </summary>
        private static BlockModel CreateItemModel()
        {
            var model = new BlockModel("item", "minecraft:apple");
            model.AddTextures(new Dictionary<string, string> { ["layer0"] = "minecraft:item/apple" });

            var texture = new Mat<Vec4b>(16, 16);
            texture.SetTo(new Scalar(255, 0, 0, 255));
            texture.GetIndexer()[0, 0] = new Vec4b(0, 0, 255, 255);
            model.AddTexureImages("layer", texture);
            return model;
        }

        // ── Output（item モデル） ─────────────────────────────────────────────

        [Fact]
        public void Output_ItemModel_WritesTextureAsPng()
        {
            using var model = CreateItemModel();
            var render = new ModelRender();
            render.SetModel(model);
            string outputDir = _temp.Combine("output");

            string fileName = render.Output(null!, outputDir);

            Assert.Equal("minecraft_apple_x16.png", fileName);
            using var img = Cv2.ImRead(Path.Combine(outputDir, fileName), ImreadModes.Unchanged);
            Assert.Equal(16, img.Width);
            Assert.Equal(16, img.Height);
            Assert.Equal(new Vec4b(0, 0, 255, 255), img.Get<Vec4b>(0, 0));
            Assert.Equal(new Vec4b(255, 0, 0, 255), img.Get<Vec4b>(15, 15));
        }

        [Theory]
        [InlineData(32, 32)]
        [InlineData(48, 32)]    // 2の累乗に切り捨てる
        [InlineData(256, 256)]
        public void Output_ItemModel_ScalesTextureByPowerOfTwo(int size, int expectedSize)
        {
            using var model = CreateItemModel();
            var render = new ModelRender();
            render.SetModel(model);
            render.Resize(size, size);

            string fileName = render.Output(null!, _temp.Path);

            Assert.Equal($"minecraft_apple_x{size}.png", fileName);
            using var img = Cv2.ImRead(_temp.Combine(fileName), ImreadModes.Unchanged);
            Assert.Equal(expectedSize, img.Width);
            // 最近傍補間で拡大されるため、左上の赤い画素は 倍率×倍率 の領域になる
            int scale = expectedSize / 16;
            Assert.Equal(new Vec4b(0, 0, 255, 255), img.Get<Vec4b>(scale - 1, scale - 1));
            Assert.Equal(new Vec4b(255, 0, 0, 255), img.Get<Vec4b>(scale, scale));
        }

        [Fact]
        public void Output_ItemModelWithoutTextures_ReturnsEmpty()
        {
            using var model = new BlockModel("item", "minecraft:air");
            var render = new ModelRender();
            render.SetModel(model);

            string fileName = render.Output(null!, _temp.Path);

            Assert.Equal("", fileName);
            Assert.Empty(Directory.GetFiles(_temp.Path));
        }

        [Fact]
        public void Output_OutputDirectoryNotExists_CreatesDirectory()
        {
            using var model = CreateItemModel();
            var render = new ModelRender();
            render.SetModel(model);
            string outputDir = _temp.Combine("a", "b");

            render.Output(null!, outputDir);

            Assert.True(File.Exists(Path.Combine(outputDir, "minecraft_apple_x16.png")));
        }
    }
}
