using System;
using UnityEditor;
using UnityEngine;

namespace Psd2UGUI.Editor
{
    /// <summary>
    /// 编辑PSD2UI组件的Inspector界面
    /// </summary>
    [CustomEditor(typeof(PSD2UI))]
    public class PSD2UIInpector : UnityEditor.Editor
    {
        private PSD2UI psd2Ui;
        private bool showWeapons;

        private void OnEnable()
        {
            psd2Ui = (PSD2UI) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();


            GUILayout.BeginHorizontal();

            if (GUILayout.Button("显示预览效果", GUILayout.Height(30)))
            {
                psd2Ui.Preview();
            }

            if (GUILayout.Button("关闭预览效果", GUILayout.Height(30)))
            {
                psd2Ui.DestroyPreview();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("导出图片", GUILayout.Height(30), GUILayout.Width(70)))
            {
                psd2Ui.ExportImage();
            }

            if (GUILayout.Button("生成UI界面", GUILayout.Height(30)))
            {
                psd2Ui.RefreshUi();
            }

            if (GUILayout.Button("清空", GUILayout.Height(30), GUILayout.Width(50)))
            {
                var title = "确认清空";
                var message = "确定要清空当前GameObject下的UI吗，将丢失页面的层级结构";

                if (EditorUtility.DisplayDialogComplex(title, message, "确定", "取消", "") == 0)
                {
                    psd2Ui.Clear();
                }
            }

            GUILayout.EndHorizontal();
        }
    }
}