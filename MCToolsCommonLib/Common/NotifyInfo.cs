using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.Common
{
    /// <summary>
    /// 通知情報を格納するクラス
    /// </summary>
    public class NotifyInfo
    {
        /// <summary>
        /// 通知メッセージ
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 通知の色コード
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public NotifyInfo()
        {
            Message = "";
            Color = "#FFFFFF";
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="message">通知メッセージ</param>
        /// <param name="color">通知の色コード</param>
        public NotifyInfo(string message, string color)
        {
            Message = message;
            Color = color;
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="info">通知情報</param>
        public NotifyInfo(NotifyInfo info)
        {
            Message = info.Message;
            Color = info.Color;
        }
    }
}
