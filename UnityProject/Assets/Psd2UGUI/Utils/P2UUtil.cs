using System.Text;
using Psd2UGUI.Element.Type;
using UnityEditor;
using UnityEngine;

namespace Psd2UGUI.Utils
{
    public class P2UUtil
    {
        /**
         * 提示框报错
         */
        private static string error = "";

        public static void AddError(string str)
        {
            if (error != "")
                error += "\n";
            error += str;
        }

        public static void ClearError()
        {
            error = "";
        }

        public static void ShowError()
        {
            if (error != "")
                EditorUtility.DisplayDialog("错误", error, "确定");
        }

        public static void ShowError(string str)
        {
            EditorUtility.DisplayDialog("错误", str, "确定");
        }

        /// <summary>
        /// 根据后缀进行类型判断
        /// </summary>
        /// <param name="suffix">Psd中图层或者组名字的后缀（名字中"@"之后的部分）</param>
        /// <param name="isImage">true：当前后缀的图层的后缀；false：当前后缀是组的后缀</param>
        /// <returns>返回后缀代表的Element类型</returns>
        public static PsdElement.ElementType GetTypeBySuffix(string suffix, bool isImage)
        {
            PsdElement.ElementType type = PsdElement.ElementType.Null;

            if (isImage)
            {
                if (suffix == "t")
                {
                    type = PsdElement.ElementType.Text;
                }
                else if (suffix == "")
                {
                    type = PsdElement.ElementType.Image;
                }
            }
            else
            {
                if (suffix == "b")
                {
                    type = PsdElement.ElementType.Button;
                }
                else if (suffix == "9")
                {
                    type = PsdElement.ElementType.Image9;
                }
                else if (suffix == "l")
                {
                    type = PsdElement.ElementType.List;
                }
                else if (suffix == "s")
                {
                    type = PsdElement.ElementType.SelectBox;
                }
                else if (suffix == "")
                {
                    type = PsdElement.ElementType.Group;
                }
            }

            return type;
        }
    }
}