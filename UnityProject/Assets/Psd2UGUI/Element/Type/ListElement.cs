using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Psd2UGUI.Utils;
using SubjectNerd.PsdImporter.PsdParser;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Psd2UGUI.Element.Type
{
    public class ListElement : PsdElement
    {
        public IPsdLayer Background;
        public TexturePiece BackgroundPiece;
        private string backgroundSuffix = "@bg";

        internal ListElement(string name, IPsdLayer layer, ElementType type, IPsdLayer[] childs) : base(name, layer,
            type, childs)
        {
            Background = FindChildElement(backgroundSuffix);
            if (Background != null && Background.HasImage)
                BackgroundPiece = new TexturePiece(name + backgroundSuffix, GetTexture2D(Background));
            else
            {
                canShow = false;
                P2UUtil.ShowError("列表:" + name + "需要有一张背景图来确定列表区域");
            }
        }

        public override void ModifyToPreview(RectTransform root, RectTransform t)
        {
            //调整大小与背景相同
            ModifySize(root, t, Background);

            //背景
            var backGround = CreateGameObject("bg", t).GetComponent<RectTransform>();
            ModifySize(root, backGround, Background);
            var bgImage = backGround.gameObject.AddComponent<Image>();
            Sprite sprite = Sprite.Create(BackgroundPiece.tex, new Rect(0, 0, Background.Width, Background.Height),
                Vector2.zero);
            bgImage.sprite = sprite;

            //列表区域
            var content = CreateGameObject("content", t).GetComponent<RectTransform>();
            ModifySize(root, content, Background);
            content.gameObject.AddComponent<RectMask2D>();
            var grid = content.gameObject.AddComponent<GridLayoutGroup>();
            var layout = GetListItems(childs);

            //计算padding
            var padding = new RectOffset(layout[new Vector2Int(1, 1)].Left - Background.Left,
                0,
                layout[new Vector2Int(1, 1)].Top - Background.Top,
                0);
            //计算spacing
            var spacing = Vector2.zero;
            var item11 = layout[new Vector2Int(1, 1)];
            if (layout.ContainsKey(new Vector2Int(2, 1)))
            {
                var item21 = layout[new Vector2Int(2, 1)];
                spacing.y = item21.Top - item11.Top - item11.Height;
            }

            if (layout.ContainsKey(new Vector2Int(1, 2)))
            {
                var item12 = layout[new Vector2Int(1, 2)];
                spacing.x = item12.Left - item11.Left - item11.Width;
            }

            //计算cell size
            var cellSize = new Vector2(item11.Width, item11.Height);

            grid.padding = padding;
            grid.spacing = spacing;
            grid.cellSize = cellSize;

            foreach (var child in childs)
            {
                if (child.Name.EndsWith(backgroundSuffix))
                    continue;
                var item = CreateGameObject(child.Name, content);
                var image = item.AddComponent<Image>();
                var itemSprite = Sprite.Create(GetTexture2D(child), new Rect(0, 0, child.Width, child.Height),
                    Vector2.zero);
                image.sprite = itemSprite;
            }
        }

        public override void ModifyToUi(RectTransform root, RectTransform t, string[] sourceDirs)
        {
            //调整大小与背景相同
            ModifySize(root, t, Background);

            //背景
            var backGround = t.Find("bg") as RectTransform;
            if (backGround == null)
            {
                backGround = CreateGameObject("bg", t).GetComponent<RectTransform>();
                backGround.gameObject.AddComponent<Image>();
            }

            ModifySize(root, backGround, Background);

            var bgImage = backGround.GetComponent<Image>();
            Sprite sprite = GetSpriteFromDirectories(BackgroundPiece.name, sourceDirs);
            bgImage.sprite = sprite;

            //列表区域
            var content = t.Find("content") as RectTransform;
            if (content == null)
            {
                content = CreateGameObject("content", t).GetComponent<RectTransform>();
                content.gameObject.AddComponent<GridLayoutGroup>();
            }

            ModifySize(root, content, Background);
            var grid = content.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                var layout = GetListItems(childs);

                //计算padding
                var padding = new RectOffset(layout[new Vector2Int(1, 1)].Left - Background.Left,
                    0,
                    layout[new Vector2Int(1, 1)].Top - Background.Top,
                    0);
                //计算spacing
                var spacing = Vector2.zero;
                var item11 = layout[new Vector2Int(1, 1)];
                var item21 = layout[new Vector2Int(2, 1)];
                if (layout.ContainsKey(new Vector2Int(1, 2)))
                {
                    var item12 = layout[new Vector2Int(1, 2)];
                    spacing.x = item12.Left - item11.Left - item11.Width;
                }

                spacing.y = item21.Top - item11.Top - item11.Height;
                //计算cell size
                var cellSize = new Vector2(item11.Width, item11.Height);

                grid.padding = padding;
                grid.spacing = spacing;
                grid.cellSize = cellSize;
            }
        }

        public override TexturePiece[] GetAllTexturePieces()
        {
            return new[] {BackgroundPiece};
        }

        //生成listItem列表
        private Dictionary<Vector2Int, IPsdLayer> GetListItems(IPsdLayer[] layers)
        {
            var layout = new Dictionary<Vector2Int, IPsdLayer>();
            foreach (var layer in layers)
            {
                string regexStr1 = "@[0-9]+_[0-9]+";
                string regexStr2 = "(?<=@).+(?=_[0-9]+)";
                string regexStr3 = "(?<=@[0-9]+_)[0-9]+";

                if (!Regex.IsMatch(layer.Name, regexStr1))
                    continue;

                var suffix = Regex.Match(layer.Name, regexStr1).ToString();
                int row = Convert.ToInt32(Regex.Match(suffix, regexStr2).ToString());
                var column = Convert.ToInt32(Regex.Match(suffix, regexStr3).ToString());
                var key = new Vector2Int(row, column);
                if (!layout.ContainsKey(key))
                    layout.Add(key, layer);
                else
                {
                    var error = "列表中:" + layer.Name + " 位置命名重复";
                    Debug.LogError(error);
                    P2UUtil.ShowError(error);
                }
            }

            return layout;
        }
    }
}