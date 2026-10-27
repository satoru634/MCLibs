using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.Common
{
    public class ProgressInfo
    {
        public int ProgressValue { get; set; }
        public string ProgressText { get; set; }

        public ProgressInfo()
        {
            ProgressValue = 0;
            ProgressText = "";
        }

        public ProgressInfo(int value, string text)
        {
            ProgressValue = value;
            ProgressText = text;
        }
    }
}
