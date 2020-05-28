using System.Collections.Generic;
using System.IO;
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
        [Range(0, 1)] public float alpha;

        PsdDocument psd;
        RectTransform rectTransform;
        Dictionary<string, Sprite> sprites;
        private GameObject preview;

        //显示图片边界
        public bool drawGimzos = false;

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
            rectTransform.sizeDelta = new Vector2(psd.Width, psd.Height);
            rectTransform.anchoredPosition = Vector2.zero;

            //根据psd图层生成map
            elementMap = new Dictionary<string, PsdElement>();
            GenerateElementMap(ref elementMap, asset.name, psd.Childs);

            //根据项目内界面生成map
            uguiElementMap = new Dictionary<string, Psd2UiElement>();
            GenerateGraphicMap(ref uguiElementMap, rectTransform);

            //生成UI
            CreateUi();
        }

        //预览UI界面
        public void Preview()
        {
            if (!gameObject.activeInHierarchy)
                return;

            LoadDocument();

            //调整页面大小
            rectTransform.sizeDelta = new Vector2(psd.Width, psd.Height);
            rectTransform.anchoredPosition = Vector2.zero;

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

            foreach (var pair in elementMap)
            {
                var element = pair.Value;
                var texturePieces = element.GetAllTexturePieces();

                foreach (var piece in texturePieces)
                {
                    if (piece == null)
                        continue;
                    var filePath = Path.Combine(exportPath + "/", piece.name + ".png");
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
            }

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

                Transform parent = null;

                //查看对应GameObject是否存在，不存在则创建一个
                GameObject go;
                if (uguiElementMap.ContainsKey(elementName))
                {
                    go = uguiElementMap[elementName].gameObject;
                    parent = go.transform.parent;
                }
                else
                {
                    go = new GameObject(elementName);
                    go.transform.SetParent(rectTransform);
                    go.AddComponent<RectTransform>().localScale = Vector3.one;
                    go.transform.SetAsLastSibling();
                }

                //将GameObject添加到子项
                go.transform.SetParent(rectTransform, true);
                RectTransform t = go.GetComponent<RectTransform>();

                //对各种类型的GameObject进行处理
                if (go.GetComponent<Psd2UiElement>() == null)
                    go.AddComponent<Psd2UiElement>().type = element.type;
                element?.ModifyToUi(rectTransform, t, new[] {exportFolderPath});

                //恢复GameObject的层级
                if (parent != null)
                    go.transform.SetParent(parent);
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
                //分析其类型
                PsdElement.ElementType type = PsdElement.ElementType.Group;

                //PsdElement.GetType(elementName);

                if (layer.HasImage)
                {
                    type = PsdElement.ElementType.Image;
                    if (elementName.EndsWith("@t"))
                    {
                        type = PsdElement.ElementType.Text;
                        elementName = elementName.Replace("@t", "");
                    }
                }
                else
                {
                    if (elementName.EndsWith("@b"))
                    {
                        type = PsdElement.ElementType.Button;
                        elementName = elementName.Replace("@b", "");
                    }
                    else if (elementName.EndsWith("@9"))
                    {
                        type = PsdElement.ElementType.Image9;
                        elementName = elementName.Replace("@9", "");
                    }
                    else if (elementName.EndsWith("@l"))
                    {
                        type = PsdElement.ElementType.List;
                        elementName = elementName.Replace("@l", "");
                    }
                    else if (elementName.EndsWith("@s"))
                    {
                        type = PsdElement.ElementType.SelectBox;
                        elementName = elementName.Replace("@s", "");
                    }
                }

                //添加子项
                var childs = layer.Childs;
                PsdElement element = PsdElement.GetPsdElement(elementName, layer, type, childs);
                if (!map.ContainsKey(element.name))
                {
                    if (type != PsdElement.ElementType.Group)
                        map.Add(elementName, element);
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
        }

        //生成Graphic图片列表
        private void GenerateGraphicMap(ref Dictionary<string, Psd2UiElement> map, RectTransform rectTrans)
        {
            Psd2UiElement[] elements = rectTrans.GetComponentsInChildren<Psd2UiElement>(true);
            Debug.Log("Graphic 数量:" + elements.Length);
            foreach (var graphic in elements)
            {
                var name = graphic.gameObject.name;
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
            if (!drawGimzos)
                return;
            Vector3[] vectors = new Vector3[4];
            foreach (var t in GetComponentsInChildren<RectTransform>())
            {
                t.GetWorldCorners(vectors);
                Gizmos.DrawLine(vectors[0], vectors[1]);
                Gizmos.DrawLine(vectors[1], vectors[2]);
                Gizmos.DrawLine(vectors[2], vectors[3]);
                Gizmos.DrawLine(vectors[3], vectors[0]);
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