using System.Text;
using UnityEditor;

namespace Psd2UGUI.Utils
{
    public class P2UUtil
    {
        public static void ShowError(string str)
        {
            EditorUtility.DisplayDialog("错误", str, "确定");
        }
    }
}