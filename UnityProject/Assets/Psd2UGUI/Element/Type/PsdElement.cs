using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SubjectNerd.PsdImporter.PsdParser;
using UnityEditor;
using UnityEngine;

namespace Psd2UGUI.Element.Type
{
    public class PsdElement
    {
        [Serializable]
        public enum ElementType
        {
            Group,
            Image,
            Image9,
            Text,
            Button,
            List,
            SelectBox
        }

        public class TexturePiece
        {
            public string name;
            public Texture2D tex;
            public bool is9Image;

            public TexturePiece(string name, Texture2D tex)
            {
                this.name = name;
                this.tex = tex;
                is9Image = false;
            }

            public TexturePiece(string name, Texture2D tex, bool is9Image) : this(name, tex)
            {
                this.is9Image = is9Image;
            }
        }

        public string name;
        public IPsdLayer layer;
        public ElementType type;
        public IPsdLayer[] childs;

        protected bool canShow;

        protected PsdElement(string name, IPsdLayer layer, ElementType type, IPsdLayer[] childs)
        {
            this.name = name;
            this.layer = layer;
            this.type = type;
            this.childs = childs;
            canShow = true;
        }

        //根据其类型生成相应的Element
        public static PsdElement GetPsdElement(string name, IPsdLayer layer, ElementType type, IPsdLayer[] childs)
        {
            if (type == ElementType.Image)
                return new ImageElement(name, layer, type, childs);
            if (type == ElementType.Image9)
                return new Image9Element(name, layer, type, childs);
            if (type == ElementType.Button)
                return new ButtonElement(name, layer, type, childs);
            if (type == ElementType.Text)
                return new TextElement(name, layer, type, childs);
            if (type == ElementType.List)
                return new ListElement(name, layer, type, childs);
            if (type == ElementType.SelectBox)
                return new SelectBoxElement(name, layer, type, childs);
            return new PsdElement(name, layer, type, childs);
        }

        //根据名字匹配对应的类型
        public static void GetType(string name)
        {
            var types = Assembly.GetCallingAssembly().GetTypes();
            var thisType = typeof(PsdElement);
            var elementList = new List<System.Type>();
            foreach (var type in types)
            {
                var baseType = type.BaseType;
                if (thisType.Name == baseType.Name)
                {
                    elementList.Add(type);
                }
            }

            foreach (var element in elementList)
            {
                Debug.LogError(element.Name);
                var obj = Activator.CreateInstance<PsdElement>();
            }
        }

        //判断当前类型
        protected static bool CheckType(string name)
        {
            return false;
        }

        //获取Element上的Texture2D
        public virtual TexturePiece[] GetAllTexturePieces()
        {
            return new TexturePiece[] { };
        }

        //生成预览
        public virtual void ModifyToPreview(RectTransform root, RectTransform t)
        {
        }

        //生成UI
        public virtual void ModifyToUi(RectTransform root, RectTransform t, string[] sourceDirs)
        {
        }

        //从文件夹获取Sprite
        protected Sprite GetSpriteFromDirectories(string name, string[] dirs)
        {
            Sprite sprite = null;

            foreach (var dir in dirs)
            {
                var path = Path.Combine(dir, name + ".png");
                if (File.Exists(path))
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            return sprite;
        }

        //调整大小
        protected void ModifySize(RectTransform root, RectTransform t, IPsdLayer layer)
        {
            var parent = t.parent;
            t.SetParent(root);

            Vector2 rootSize = root.rect.size;
            Vector2 rootOffest = (Vector2) (root.position) - rootSize / 2f;

            var texLayer = layer;
            t.anchoredPosition = new Vector2(texLayer.Left + texLayer.Width / 2f,
                                     rootSize.y - (texLayer.Top + texLayer.Height / 2f) - 1) +
                                 rootOffest;
            t.sizeDelta = new Vector2(texLayer.Width, texLayer.Height);

            t.SetParent(parent);
        }

        //创建一个GameObject
        protected GameObject CreateGameObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            var rectTransform = go.AddComponent<RectTransform>();
            rectTransform.parent = parent;
            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = Vector3.zero;
            return go;
        }

        //找到子项
        protected IPsdLayer FindChildElement(string suffix)
        {
            IPsdLayer childElement = null;

            foreach (var child in childs)
            {
                if (child.Name.EndsWith(suffix))
                {
                    childElement = child;
                }
            }

            return childElement;
        }

        //获取Texture2D
        protected Texture2D GetTexture2D(IPsdLayer layer)
        {
            byte[] data = layer.MergeChannels();
            var channelCount = layer.Channels.Length;
            var pitch = layer.Width * layer.Channels.Length;
            var w = layer.Width;
            var h = layer.Height;

            var format = channelCount == 3 ? TextureFormat.RGB24 : TextureFormat.ARGB32;
            var tex = new Texture2D(w, h, format, false);
            var colors = new Color32[data.Length / channelCount];

            var k = 0;
            for (var y = h - 1; y >= 0; --y)
            {
                for (var x = 0; x < pitch; x += channelCount)
                {
                    var n = x + y * pitch;
                    var c = new Color32();
                    if (channelCount == 5)
                    {
                        c.b = data[n++];
                        c.g = data[n++];
                        c.r = data[n++];
                        n++;
                        c.a = (byte) Mathf.RoundToInt((float) (data[n++]) * layer.Opacity);
                    }
                    else if (channelCount == 4)
                    {
                        c.b = data[n++];
                        c.g = data[n++];
                        c.r = data[n++];
                        c.a = (byte) Mathf.RoundToInt((float) data[n++] * layer.Opacity);
                    }
                    else
                    {
                        c.b = data[n++];
                        c.g = data[n++];
                        c.r = data[n++];
                        c.a = (byte) Mathf.RoundToInt(layer.Opacity * 255f);
                    }

                    colors[k++] = c;
                }
            }

            tex.SetPixels32(colors);
            tex.Apply(false, false);
            return tex;
        }
    }
}