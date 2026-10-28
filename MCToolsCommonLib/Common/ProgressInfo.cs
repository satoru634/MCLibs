using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.Common
{
    /// <summary>
    /// 進捗情報を格納するクラス
    /// </summary>
    public class ProgressInfo
    {
        /// <summary>
        /// 進捗状況を表すプロパティ
        /// </summary>
        public int ProgressValue { get; set; }

        /// <summary>
        /// 進捗のテキストを表すプロパティ
        /// </summary>
        public string ProgressText { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public ProgressInfo()
        {
            ProgressValue = 0;
            ProgressText = "";
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="value">進捗状況</param>
        /// <param name="text">進捗のテキスト</param>
        public ProgressInfo(int value, string text)
        {
            ProgressValue = value;
            ProgressText = text;
        }
    }
}
