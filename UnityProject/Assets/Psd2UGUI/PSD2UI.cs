﻿using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Psd2UGUI.Element;
using Psd2UGUI.Element.Type;
using Psd2UGUI.Utils;
using SubjectNerd.PsdImporter.PsdParser;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Directory = UnityEngine.Windows.Directory;
using Object = UnityEngine.Object;

namespace Psd2UGUI
{
    [ExecuteInEditMode]
    public class PSD2UI : MonoBehaviour
    {
        //psd文件
        public Object asset;
        //导出文件夹
        public Object exportFolder;
        //预览图透明度
        [Range(0, 1)] public float alpha = 0.3f;

        PsdDocument psd;
        RectTransform rectTransform;
        Dictionary<string, Sprite> sprites;
        private GameObject preview;

        //显示图片边界
        //public bool drawGimzos = false;
        //显示RayCast区域
        public bool drawRaycast = false;

        //psd图片Map
        private Dictionary<string, PsdElement> elementMap;

        //页面图片Map
        private Dictionary<string, Psd2UiElement> uguiElementMap;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = gameObject.AddComponent<RectTransform>();
        }

        public void RefreshUi()
        {
            if (!gameObject.activeInHierarchy)
                return;

            LoadDocument();
            GenerateUi();
        }

        public void LoadDocument()
        {
            UnLoadDocument();
#if UNITY_EDITOR
            if (asset != null)
            {
                string path = AssetDatabase.GetAssetPath(asset);
                if (path.EndsWith(".psd"))
                    psd = PsdDocument.Create(path);
            }
#endif
        }

        /// <summary>
        /// 界面接口
        /// </summary>

        //生成UI界面
        public void GenerateUi()
        {
            if (psd == null)
                return;

            //调整页面大小
            var offset = rectTransform.anchorMax - rectTransform.anchorMin;
            var psdSize = new Vector2(psd.Width, psd.Height);
            rectTransform.sizeDelta = new Vector2(psd.Width, psd.Height) * (Vector2.one - offset);
            rectTransform.anchoredPosition = ((Vector2.one - offset) / 2 - rectTransform.anchorMin) * psdSize;

            //根据psd图层生成map
            elementMap = new Dictionary<string, PsdElement>();
            GenerateElementMap(ref elementMap, asset.name, psd.Childs);

            //根据项目内界面生成map
            uguiElementMap = new Dictionary<string, Psd2UiElement>();
            GenerateGraphicMap(ref uguiElementMap, rectTransform);

            //生成UI
            CreateUi();

            //将预览界面提到最前
            if (preview != null)
                preview.transform.SetAsLastSibling();
        }

        //预览UI界面
        public void Preview()
        {
            if (!gameObject.activeInHierarchy)
                return;

            LoadDocument();

            //调整页面大小
            var offset = rectTransform.anchorMax - rectTransform.anchorMin;
            var psdSize = new Vector2(psd.Width, psd.Height);
            rectTransform.sizeDelta = new Vector2(psd.Width, psd.Height) * (Vector2.one - offset);
            rectTransform.anchoredPosition = ((Vector2.one - offset) / 2 - rectTransform.anchorMin) * psdSize;

            DestroyPreview();

            preview = new GameObject("preview");

            var previewRectTransform = preview.AddComponent<RectTransform>();
            preview.AddComponent<CanvasGroup>().alpha = alpha;
            //调整页面大小
            previewRectTransform.sizeDelta = new Vector2(psd.Width, psd.Height);
            previewRectTransform.SetAsLastSibling();
            previewRectTransform.SetParent(rectTransform);
            previewRectTransform.localScale = Vector3.one;
            previewRectTransform.localPosition = Vector3.zero;
            preview.AddComponent<RectMask2D>();

            elementMap = new Dictionary<string, PsdElement>();
            GenerateElementMap(ref elementMap, asset.name, psd.Childs);

            GeneratePreview(previewRectTransform, previewRectTransform, psd.Childs);
        }

        //销毁预览UI界面
        public void DestroyPreview()
        {
            var previewRectTransform = rectTransform.Find("preview");
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
        public void ExportImage()
        {
            LoadDocument();

            if (exportFolder == null)
            {
                Debug.Log("导出文件夹为空");
                return;
            }

            string exportPath = AssetDatabase.GetAssetPath(exportFolder);

            if (!Directory.Exists(exportPath))
            {
                Debug.Log("导出文件夹填入错误");
                return;
            }

            elementMap = new Dictionary<string, PsdElement>();
            GenerateElementMap(ref elementMap, asset.name, psd.Childs);

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
            if (rectTransform != null)
            {
                int count = rectTransform.childCount;
                for (int i = count - 1; i >= 0; i--)
                {
                    DestroyImmediate(rectTransform.GetChild(i).gameObject);
                }
            }
        }

        /// <summary>
        /// 私有方法
        /// </summary>

        //生成UI界面
        private void CreateUi()
        {
            if (elementMap == null || uguiElementMap == null)
                return;
            //图片资源路径
            var exportFolderPath = AssetDatabase.GetAssetPath(exportFolder);

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
                    go.transform.SetParent(rectTransform);
                    go.AddComponent<RectTransform>().localScale = Vector3.one;
                    go.transform.SetAsLastSibling();
                }

                //将GameObject添加到子项
                go.transform.SetParent(rectTransform, true);
                RectTransform t = go.GetComponent<RectTransform>();

                //对各种类型的GameObject进行处理
                if (go.GetComponent<Psd2UiElement>() == null)
                {
                    var psdUiElement = go.AddComponent<Psd2UiElement>();
                    psdUiElement.type = element.type;
                    psdUiElement.elementName = elementName;
                }

                element?.ModifyToUi(rectTransform, t, new[] {exportFolderPath});

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
                t.SetParent(rectTransform);
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

        private void OnDestroy()
        {
            UnLoadDocument();
            Clear();
            DestroyPreview();
        }

        private void OnDrawGizmos()
        {
//            if (drawGimzos)
//            {
//                Vector3[] vectors = new Vector3[4];
//                foreach (var t in GetComponentsInChildren<RectTransform>())
//                {
//                    t.GetWorldCorners(vectors);
//                    Gizmos.DrawLine(vectors[0], vectors[1]);
//                    Gizmos.DrawLine(vectors[1], vectors[2]);
//                    Gizmos.DrawLine(vectors[2], vectors[3]);
//                    Gizmos.DrawLine(vectors[3], vectors[0]);
//                }
//            }

            if (drawRaycast)
            {
                foreach (MaskableGraphic g in FindObjectsOfType<MaskableGraphic>())
                {
                    if (g.raycastTarget)
                    {
                        Gizmos.color = Color.white;
                        Vector3[] vectors = new Vector3[4];
                        RectTransform rectTransform = g.transform as RectTransform;
                        if (rectTransform != null) rectTransform.GetWorldCorners(vectors);
                        for (int i = 0; i < 4; i++)
                        {
                            Gizmos.DrawLine(vectors[i], vectors[(i + 1) % 4]);
                        }

//                        Gizmos.color = new Color(0, 0, 1, 0.2f);
//                        Gizmos.DrawCube((vectors[2] + vectors[0]) / 2, vectors[2] - vectors[0]);
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (preview != null)
            {
                preview.GetComponent<CanvasGroup>().alpha = alpha;
            }

            //EditorApplication.delayCall += RefreshUi;
        }

        [InitializeOnLoadMethod]
        static void AutoCreateMethod()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemCallback;
        }

        static void HierarchyWindowItemCallback(int pID, Rect pRect)
        {
            if (!pRect.Contains(Event.current.mousePosition))
                return;

            GameObject targetGo = EditorUtility.InstanceIDToObject(pID) as GameObject;
            if (targetGo == null || targetGo.GetComponentInParent<Canvas>() == null)
                return;

            if (Event.current.type == EventType.DragUpdated)
            {
                foreach (string path in DragAndDrop.paths)
                {
                    if (!string.IsNullOrEmpty(path) && path.EndsWith(".psd"))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        DragAndDrop.AcceptDrag();
                        Event.current.Use();
                    }
                }
            }
            else if (Event.current.type == EventType.DragPerform)
            {
                foreach (string path in DragAndDrop.paths)
                {
                    if (!string.IsNullOrEmpty(path) && path.EndsWith(".psd"))
                    {
                        Object asset = AssetDatabase.LoadMainAssetAtPath(path);
                        GameObject go = new GameObject(asset.name);
                        go.transform.SetParent(targetGo.transform, false);
                        go.AddComponent<PSD2UI>().asset = asset;
                        Event.current.Use();
                    }
                }
            }
        }

//        [MenuItem("GameObject/UI/Create Text By Name", false)]
//        public static void CreateTextByName()
//        {
//            GameObject[] gameObjects = Selection.gameObjects;
//            foreach (GameObject go in gameObjects)
//            {
//                Graphic g = go.GetComponent<Graphic>();
//                if (g != null)
//                    GameObject.DestroyImmediate(g);
//                Text t = go.AddComponent<Text>();
//                t.verticalOverflow = VerticalWrapMode.Overflow;
//                t.text = go.name;
//            }
//        }

#endif
    }
}