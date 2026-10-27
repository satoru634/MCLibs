using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.Utils
{
    public class ConvertJsonValue
    {
        /// <summary>
        /// JSONオブジェクトをintに変換
        /// </summary>
        /// <param name="orgInt">整数値(JSON形式)</param>
        /// <returns>整数値</returns>
        static public int ConvertInt(object orgInt)
        {
            string? strInt = orgInt.ToString();
            if (strInt == null)
            {
                return 0;
            }

            return int.Parse(strInt);
        }

        /// <summary>
        /// JSONオブジェクトをdoubleに変換
        /// </summary>
        /// <param name="orgDouble">浮動小数点(JSON形式)</param>
        /// <returns>浮動小数点</returns>
        static public double ConvertDouble(object orgDouble)
        {
            string? strDouble = orgDouble.ToString();
            if (strDouble == null)
            {
                return 0.0f;
            }

            return double.Parse(strDouble);
        }

        /// <summary>
        /// JSONオブジェクトを文字列に変換
        /// </summary>
        /// <param name="orgStr">文字列(JSON形式)</param>
        /// <returns>文字列</returns>
        static public string ConvertStr(object orgStr)
        {
            string? str = orgStr.ToString();
            if (str == null)
            {
                return "";
            }

            return str;
        }
    }
}
