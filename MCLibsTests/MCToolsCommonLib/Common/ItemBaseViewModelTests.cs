using MCToolsCommonLib.Common;

namespace MCLibsTests.MCToolsCommonLib.Common
{
    /// <summary>
    /// ItemBaseViewModel のテスト
    /// </summary>
    public class ItemBaseViewModelTests
    {
        // ── コンストラクター ───────────────────────────────────────────────────

        [Fact]
        public void Constructor_Default_InitializesEmptyState()
        {
            var model = new ItemBaseViewModel<string>();

            Assert.Empty(model.ItemList);
            Assert.Equal(-1, model.SelectedIndex);
            Assert.False(model.Enable);
        }

        [Fact]
        public void Constructor_CopyFromExisting_CopiesState()
        {
            var source = new ItemBaseViewModel<string>();
            source.Init(["a", "b"], "b");

            var copy = new ItemBaseViewModel<string>(source);

            Assert.Same(source.ItemList, copy.ItemList);
            Assert.Equal(1, copy.SelectedIndex);
            Assert.True(copy.Enable);
        }

        // ── Init ──────────────────────────────────────────────────────────────

        [Fact]
        public void Init_WithExistingSelectedItem_SelectsMatchingIndex()
        {
            var model = new ItemBaseViewModel<string>();

            model.Init(["a", "b", "c"], "c");

            Assert.Equal(["a", "b", "c"], model.ItemList);
            Assert.Equal(2, model.SelectedIndex);
            Assert.True(model.Enable);
        }

        [Fact]
        public void Init_WithUnknownSelectedItem_SelectsFirstIndex()
        {
            var model = new ItemBaseViewModel<string>();

            model.Init(["a", "b"], "unknown");

            Assert.Equal(0, model.SelectedIndex);
        }

        [Fact]
        public void Init_WithEmptyList_SelectedIndexIsMinusOneAndKeepsPreviousItems()
        {
            var model = new ItemBaseViewModel<string>();
            model.Init(["a"], "a");

            model.Init([], "a");

            // 空リストの場合は選択だけ解除し、リストと有効状態は変更しない
            Assert.Equal(-1, model.SelectedIndex);
            Assert.Equal(["a"], model.ItemList);
            Assert.True(model.Enable);
        }

        [Fact]
        public void Init_CalledTwice_ReplacesItems()
        {
            var model = new ItemBaseViewModel<int>();
            model.Init([1, 2], 1);

            model.Init([10, 20, 30], 20);

            Assert.Equal([10, 20, 30], model.ItemList);
            Assert.Equal(1, model.SelectedIndex);
        }

        // ── Add ───────────────────────────────────────────────────────────────

        [Fact]
        public void Add_AppendsItemWithoutChangingSelection()
        {
            var model = new ItemBaseViewModel<string>();

            model.Add("a");
            model.Add("b");

            Assert.Equal(["a", "b"], model.ItemList);
            Assert.Equal(-1, model.SelectedIndex);
            Assert.False(model.Enable);
        }

        // ── Clear ─────────────────────────────────────────────────────────────

        [Fact]
        public void Clear_ResetsAllState()
        {
            var model = new ItemBaseViewModel<string>();
            model.Init(["a", "b"], "b");

            model.Clear();

            Assert.Empty(model.ItemList);
            Assert.Equal(-1, model.SelectedIndex);
            Assert.False(model.Enable);
        }
    }
}
