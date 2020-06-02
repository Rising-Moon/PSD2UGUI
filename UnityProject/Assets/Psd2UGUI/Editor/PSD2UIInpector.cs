using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Psd2UGUI.Element;
using Psd2UGUI.Element.Type;
using UnityEditor;
using UnityEngine;
using Psd2UGUI.Utils;
using SubjectNerd.PsdImporter.PsdParser;
using UnityEngine.UI;

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

        PsdDocument psd;
        Dictionary<string, Sprite> sprites;
        private GameObject preview;

        //显示图片边界
        //public bool drawGimzos = false;

        //psd图片Map
        private Dictionary<string, PsdElement> elementMap;

        //页面图片Map
        private Dictionary<string, Psd2UiElement> uguiElementMap;

        private void OnEnable()
        {
            psd2Ui = (PSD2UI) target;
        }

        private void OnDisable()
        {
            UnLoadDocument();
            DestroyPreview();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();


            GUILayout.BeginHorizontal();

            if (GUILayout.Button("显示预览效果", GUILayout.Height(30)))
            {
                Preview();
            }

            if (GUILayout.Button("关闭预览效果", GUILayout.Height(30)))
            {
                DestroyPreview();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("导出图片", GUILayout.Height(30), GUILayout.Width(70)))
            {
                ExportImage(AssetDatabase.GetAssetPath(psd2Ui.exportFolder));
            }

            if (GUILayout.Button("生成UI界面", GUILayout.Height(30)))
            {
                RefreshUi(AssetDatabase.GetAssetPath(psd2Ui.exportFolder));
            }

            if (GUILayout.Button("清空", GUILayout.Height(30), GUILayout.Width(50)))
            {
                var title = "确认清空";
                var message = "确定要清空当前GameObject下的UI吗，将丢失页面的层级结构";

                if (EditorUtility.DisplayDialogComplex(title, message, "确定", "取消", "") == 0)
                {
                    Clear();
                }
            }

            GUILayout.EndHorizontal();
        }

        public void RefreshUi(string path)
        {
            if (!psd2Ui.gameObject.activeInHierarchy)
                return;

            LoadDocument();
            GenerateUi(path);
        }

        public void LoadDocument()
        {
            UnLoadDocument();
#if UNITY_EDITOR
            if (psd2Ui.asset != null)
            {
                string path = AssetDatabase.GetAssetPath(psd2Ui.asset);
                if (path.EndsWith(".psd"))
                    psd = PsdDocument.Create(path);
            }
#endif
        }

        /// <summary>
        /// 界面接口
        /// </summary>

        //生成UI界面
        public void GenerateUi(string path)
        {
            if (psd == null)
                return;

            //调整页面大小
            var offset = psd2Ui.rectTransform.anchorMax - psd2Ui.rectTransform.anchorMin;
            var psdSize = new Vector2(psd.Width, psd.Height);
            psd2Ui.rectTransform.sizeDelta = new Vector2(psd.Width, psd.Height) * (Vector2.one - offset);
            psd2Ui.rectTransform.anchoredPosition = ((Vector2.one - offset) / 2 - psd2Ui.rectTransform.anchorMin) * psdSize;

            //根据psd图层生成map
            elementMap = new Dictionary<string, PsdElement>();
            GenerateElementMap(ref elementMap, psd2Ui.asset.name, psd.Childs);

            //根据项目内界面生成map
            uguiElementMap = new Dictionary<string, Psd2UiElement>();
            GenerateGraphicMap(ref uguiElementMap, psd2Ui.rectTransform);

            //生成UI
            CreateUi(path);

            //将预览界面提到最前
            if (preview != null)
                preview.transform.SetAsLastSibling();
        }

        //预览UI界面
        public void Preview()
        {
            if (!psd2Ui.gameObject.activeInHierarchy)
                return;

            LoadDocument();

            //调整页面大小
            var offset = psd2Ui.rectTransform.anchorMax - psd2Ui.rectTransform.anchorMin;
            var psdSize = new Vector2(psd.Width, psd.Height);
            psd2Ui.rectTransform.sizeDelta = new Vector2(psd.Width, psd.Height) * (Vector2.one - offset);
            psd2Ui.rectTransform.anchoredPosition = ((Vector2.one - offset) / 2 - psd2Ui.rectTransform.anchorMin) * psdSize;

            DestroyPreview();

            preview = new GameObject("preview");

            var previewRectTransform = preview.AddComponent<RectTransform>();
            preview.AddComponent<CanvasGroup>().alpha = psd2Ui.alpha;
            //调整页面大小
            previewRectTransform.sizeDelta = new Vector2(psd.Width, psd.Height);
            previewRectTransform.SetAsLastSibling();
            previewRectTransform.SetParent(psd2Ui.rectTransform);
            previewRectTransform.localScale = Vector3.one;
            previewRectTransform.localPosition = Vector3.zero;
            preview.AddComponent<RectMask2D>();

            elementMap = new Dictionary<string, PsdElement>();
            GenerateElementMap(ref elementMap, psd2Ui.asset.name, psd.Childs);

            GeneratePreview(previewRectTransform, previewRectTransform, psd.Childs);
        }

        //销毁预览UI界面
        public void DestroyPreview()
        {
            var previewRectTransform = psd2Ui.rectTransform.Find("preview");
            if (previewRectTransform == null)
                return;
            preview = previewRectTransform.gameObject;

            if (preview != null)
            {
                DestroyImmediate(preview);
                preview = null;
            }
        }

        //导出图片资源
        public void ExportImage(string path)
        {
            LoadDocument();

            if (psd2Ui.exportFolder == null)
            {
                Debug.Log("导出文件夹为空");
                return;
            }

            string exportPath = path;

            if (!Directory.Exists(exportPath))
            {
                Debug.Log("导出文件夹填入错误");
                return;
            }

            elementMap = new Dictionary<string, PsdElement>();
            GenerateElementMap(ref elementMap, psd2Ui.asset.name, psd.Childs);

            //导出图片进度
            var progress = 0f;
            var section = 1f / elementMap.Count;

            foreach (var pair in elementMap)
            {
                var element = pair.Value;
                var texturePieces = element.GetAllTexturePieces();

                foreach (var piece in texturePieces)
                {
                    if (piece == null)
                        continue;
                    var filePath = Path.Combine(exportPath + "/", piece.name + ".png");
                    EditorUtility.DisplayProgressBar("导出图片", piece.name + ".png", progress);
                    if (!File.Exists(filePath))
                    {
                        File.Create(filePath).Dispose();
                    }

                    File.WriteAllBytes(filePath, piece.tex.EncodeToPNG());
                    AssetDatabase.Refresh();
                    TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
                    if (importer != null)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        if (piece.is9Image)
                        {
                            if (piece.tex != null)
                                importer.spriteBorder = new Vector4(
                                    Mathf.Floor(f: piece.tex.width / 2),
                                    Mathf.Floor(f: piece.tex.height / 2),
                                    Mathf.Floor(f: piece.tex.width / 2),
                                    Mathf.Floor(f: piece.tex.height / 2));
                        }

                        importer.SaveAndReimport();
                    }
                }

                progress += section;
                EditorUtility.DisplayProgressBar("导出图片", "", progress);
            }

            EditorUtility.ClearProgressBar();

            Debug.Log("导出完成");
        }

        //清空界面
        public void Clear()
        {
            if (psd2Ui.rectTransform != null)
            {
                int count = psd2Ui.rectTransform.childCount;
                for (int i = count - 1; i >= 0; i--)
                {
                    DestroyImmediate(psd2Ui.rectTransform.GetChild(i).gameObject);
                }
            }
        }

        /// <summary>
        /// 私有方法
        /// </summary>

        //生成UI界面
        private void CreateUi(string path)
        {
            if (elementMap == null || uguiElementMap == null)
                return;
            //图片资源路径
            var exportFolderPath = path;

            //遍历elementMap生成UI
            foreach (var pair in elementMap)
            {
                var elementName = pair.Key;
                var element = pair.Value;

                if (!element.canShow)
                    continue;

                Transform parent = null;
                int siblingIndex = -1;

                //查看对应GameObject是否存在，不存在则创建一个
                GameObject go;
                if (uguiElementMap.ContainsKey(elementName))
                {
                    go = uguiElementMap[elementName].gameObject;
                    parent = go.transform.parent;
                    siblingIndex = go.transform.GetSiblingIndex();
                    Psd2UiElement psd2UiElement = uguiElementMap[elementName];
                    //当UI没有勾选对应到PSD时则不进行处理
                    if (!psd2UiElement.linkPsd)
                        continue;
                }
                else
                {
                    var name = elementName.Replace("-", "/");
                    go = new GameObject(name);
                    go.transform.SetParent(psd2Ui.rectTransform);
                    go.AddComponent<RectTransform>().localScale = Vector3.one;
                    go.transform.SetAsLastSibling();
                }

                //将GameObject添加到子项
                go.transform.SetParent(psd2Ui.rectTransform, true);
                RectTransform t = go.GetComponent<RectTransform>();

                //对各种类型的GameObject进行处理
                if (go.GetComponent<Psd2UiElement>() == null)
                {
                    var psdUiElement = go.AddComponent<Psd2UiElement>();
                    psdUiElement.type = element.type;
                    psdUiElement.elementName = elementName;
                }

                element?.ModifyToUi(psd2Ui.rectTransform, t, new[] {exportFolderPath});

                //恢复GameObject的层级
                if (parent != null)
                {
                    go.transform.SetParent(parent);
                    go.transform.SetSiblingIndex(siblingIndex);
                }

                var locaPosition = t.localPosition;
                t.localPosition = new Vector3(locaPosition.x, locaPosition.y, 0);

                //根据psd中的是否可视，调整Active
                go.SetActive(element.layer is PsdLayer ? (element.layer as PsdLayer).IsVisible : true);
            }
        }

        private void GeneratePreview(RectTransform previewRectTransform, RectTransform parent, IPsdLayer[] layers)
        {
            if (layers == null)
                return;

            foreach (var pair in elementMap)
            {
                var elementName = pair.Key;
                var element = pair.Value;

                if (!element.canShow)
                    continue;

                GameObject go = null;
                go = new GameObject(elementName);
                RectTransform t = go.AddComponent<RectTransform>();
                t.SetParent(psd2Ui.rectTransform);
                t.localScale = Vector3.one;

                //创建预览界面
                element?.ModifyToPreview(previewRectTransform, t);

                t.SetParent(parent, true);
                var localPosition = t.localPosition;
                t.localPosition = new Vector3(localPosition.x, localPosition.y, 0);
                go.SetActive(element.layer is PsdLayer ? (element.layer as PsdLayer).IsVisible : true);
            }
        }

        //生成psd元素列表
        private void GenerateElementMap(ref Dictionary<string, PsdElement> map, string parentName, IPsdLayer[] layers)
        {
            if (layers == null)
                return;

            foreach (var layer in layers)
            {
                var layerName = parentName + "-" + layer.Name.Replace(" ", "");
                var elementName = layerName;

                //解析为PsdElement

                //正则获取后缀与名字
                var suffixRegex = "(?<=@).+";
                var nameRegex = ".+(?=@)";
                var suffix = "";
                var name = elementName;
                if (Regex.IsMatch(elementName, suffixRegex))
                {
                    suffix = Regex.Match(elementName, suffixRegex).ToString();
                    name = Regex.Match(elementName, nameRegex).ToString();
                }

                if (suffix == "@")
                    continue;
                var type = P2UUtil.GetTypeBySuffix(suffix, layer.HasImage);

                if (type == PsdElement.ElementType.Null)
                {
                    Debug.LogError(elementName + "后缀使用错误");
                    P2UUtil.AddError(elementName + "后缀使用错误");
                    continue;
                }

                //添加子项
                var childs = layer.Childs;
                PsdElement element = P2UUtil.GetPsdElement(name, layer, type, childs);
                if (!map.ContainsKey(element.name))
                {
                    if (type != PsdElement.ElementType.Group)
                        map.Add(name, element);
                }
                else
                {
                    string error = "图层命名重复:" + layerName;
                    error = error.Replace("-", "/");
                    P2UUtil.ShowError(error);
                    Debug.LogError("图层命名重复:" + layerName);
                }

                if (element.type == PsdElement.ElementType.Group)
                    GenerateElementMap(ref map, layerName, layer.Childs);
            }

            P2UUtil.ShowError();
            P2UUtil.ClearError();
        }

        //生成Graphic图片列表
        private void GenerateGraphicMap(ref Dictionary<string, Psd2UiElement> map, RectTransform rectTrans)
        {
            Psd2UiElement[] elements = rectTrans.GetComponentsInChildren<Psd2UiElement>(true);
            foreach (var graphic in elements)
            {
                var name = graphic.elementName;
                if (!map.ContainsKey(name))
                {
                    map.Add(name, graphic);
                }
                else
                {
                    Debug.LogError("Graphic重复:" + name);
                }
            }
        }

        private void UnLoadDocument()
        {
            if (psd != null)
            {
                psd.Dispose();
                psd = null;
            }
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (!psd2Ui.gameObject.activeInHierarchy)
                return;

            if (preview != null)
            {
                preview.GetComponent<CanvasGroup>().alpha = psd2Ui.alpha;
            }
        }
    }
#endif
}