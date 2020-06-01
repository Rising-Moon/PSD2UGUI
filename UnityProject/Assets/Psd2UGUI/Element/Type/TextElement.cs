using System.Text.RegularExpressions;
using SubjectNerd.PsdImporter.PsdParser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Psd2UGUI.Element.Type
{
    public class TextElement : PsdElement
    {
        public IPsdLayer text;
        public TexturePiece textPiece;

        internal TextElement(string name, IPsdLayer layer, ElementType type, IPsdLayer[] childs) : base(name, layer,
            type, childs)
        {
            text = layer;
            textPiece = new TexturePiece(name, GetTexture2D(layer), type == ElementType.Image9);
        }

        //生成Preview
        public override void ModifyToPreview(RectTransform root, RectTransform t)
        {
            ModifySize(root, t, text);

            Image image = t.gameObject.AddComponent<Image>();
            Texture2D tex = textPiece.tex;
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, text.Width, text.Height), Vector2.zero);
            image.sprite = sprite;
        }

        //生成UI
        public override void ModifyToUi(RectTransform root, RectTransform t, string[] sourceDirs)
        {
            TextMeshProUGUI text = t.GetComponent<TextMeshProUGUI>();
            if (text == null)
            {
                text = t.gameObject.AddComponent<TextMeshProUGUI>();
                ModifySize(root, t, this.text);
            }

            if (text.text == default)
            {
                text.text = Regex.Match(name, "(?!.*-).+").ToString();
            }
        }
    }
}