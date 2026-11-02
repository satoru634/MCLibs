using System.Text.Json;

namespace MCLibsTests.Helpers
{
    /// <summary>
    /// JSON 関連のテスト用ヘルパー
    /// </summary>
    public static class TestJson
    {
        /// <summary>
        /// JSON 文字列を object（実体は JsonElement）に変換する。
        /// プロダクトコードが Dictionary&lt;string, object&gt; から受け取る値と同じ形にするために使う。
        /// </summary>
        /// <param name="json">JSON 文字列</param>
        /// <returns>JsonElement を格納した object</returns>
        public static object ToObject(string json)
        {
            return JsonSerializer.Deserialize<object>(json)!;
        }
    }
}
