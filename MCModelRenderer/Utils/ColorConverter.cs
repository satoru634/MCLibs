using OpenCvSharp;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WpfColor = System.Windows.Media.Color;

namespace MCModelRenderer.Utils
{
    /// <summary>
    /// 色を変換するためのユーティリティクラス。
    /// </summary>
    public class ColorConverter
    {
        /// <summary>
        /// 文字列からColor4に変換するメソッド。
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        static public Color4 StringToColor4(string color)
        {
            if (color.Length == 7)
            {                 // #RRGGBB形式
                byte r = Convert.ToByte(color.Substring(1, 2), 16);
                byte g = Convert.ToByte(color.Substring(3, 2), 16);
                byte b = Convert.ToByte(color.Substring(5, 2), 16);
                return new Color4(r / 255f, g / 255f, b / 255f, 1.0f);
            }
            else if (color.Length == 9)
            {
                // #AARRGGBB形式
                byte a = Convert.ToByte(color.Substring(1, 2), 16);
                byte r = Convert.ToByte(color.Substring(3, 2), 16);
                byte g = Convert.ToByte(color.Substring(5, 2), 16);
                byte b = Convert.ToByte(color.Substring(7, 2), 16);
                return new Color4(r / 255f, g / 255f, b / 255f, a / 255f);
            }

            // デフォルトは黒色
            return new Color4(0, 0, 0, 0);
        }

        /// <summary>
        /// 文字列からWpfColorに変換するメソッド。
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        static public WpfColor StringToWpfColor(string color)
        {
            if (color.Length == 7)
            {
                // #RRGGBB形式
                byte r = Convert.ToByte(color.Substring(1, 2), 16);
                byte g = Convert.ToByte(color.Substring(3, 2), 16);
                byte b = Convert.ToByte(color.Substring(5, 2), 16);
                return WpfColor.FromArgb(255, r, g, b); // ColorはARGB形式
            }
            else if (color.Length == 9)
            {
                // #AARRGGBB形式
                byte a = Convert.ToByte(color.Substring(1, 2), 16);
                byte r = Convert.ToByte(color.Substring(3, 2), 16);
                byte g = Convert.ToByte(color.Substring(5, 2), 16);
                byte b = Convert.ToByte(color.Substring(7, 2), 16);
                return WpfColor.FromArgb(a, r, g, b); // ColorはARGB形式
            }

            // デフォルトは黒色
            return WpfColor.FromArgb(255, 0, 0, 0);
        }

        /// <summary>
        /// 文字列からVec4bに変換するメソッド。
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        static public Vec4b StringToVec4b(string color)
        {
            if (color.Length == 7)
            {
                // #RRGGBB形式
                byte r = Convert.ToByte(color.Substring(1, 2), 16);
                byte g = Convert.ToByte(color.Substring(3, 2), 16);
                byte b = Convert.ToByte(color.Substring(5, 2), 16);
                return new Vec4b(b, g, r, 255); // Vec4bはBGRA形式
            }
            else if (color.Length == 9)
            {
                // #AARRGGBB形式
                byte a = Convert.ToByte(color.Substring(1, 2), 16);
                byte r = Convert.ToByte(color.Substring(3, 2), 16);
                byte g = Convert.ToByte(color.Substring(5, 2), 16);
                byte b = Convert.ToByte(color.Substring(7, 2), 16);
                return new Vec4b(b, g, r, a); // Vec4bはBGRA形式
            }

            // デフォルトは黒色
            return new Vec4b(0, 0, 0, 255);
        }

        /// <summary>
        /// Color4からVec4b (BGRA) へ変換するメソッド。
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        static public Vec4b Color4ToVec4b(Color4 color)
        {
            // Color4からVec4b (BGRA) へ変換
            byte b = (byte)(color.Blue * 255);
            byte g = (byte)(color.Green * 255);
            byte r = (byte)(color.Red * 255);
            byte a = (byte)(color.Alpha * 255);
            return new Vec4b(b, g, r, a);
        }

        /// <summary>
        /// 文字列が#RRGGBBまたは#AARRGGBB形式かをチェックするメソッド。
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        static public bool CheckColorFormat(string color)
        {
            // 文字列が#RRGGBBまたは#AARRGGBB形式かをチェック
            if (color.Length == 7 || color.Length == 9)
            {
                if (color[0] == '#')
                {
                    for (int i = 1; i < color.Length; i++)
                    {
                        if (!Uri.IsHexDigit(color[i]))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
            return false;
        }
    }
}
