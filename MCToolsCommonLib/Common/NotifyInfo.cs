using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.Common
{
    public class NotifyInfo
    {
        public string Message { get; set; }
        public string Color { get; set; }

        public NotifyInfo()
        {
            Message = "";
            Color = "#FFFFFF";
        }

        public NotifyInfo(string message, string color)
        {
            Message = message;
            Color = color;
        }

        public NotifyInfo(NotifyInfo info)
        {
            Message = info.Message;
            Color = info.Color;
        }
    }
}
