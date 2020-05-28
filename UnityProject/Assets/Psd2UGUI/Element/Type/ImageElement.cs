using SubjectNerd.PsdImporter.PsdParser;
using UnityEngine;
using UnityEngine.UI;

namespace Psd2UGUI.Element.Type
{
    public class ImageElement : PsdElement
    {
        public IPsdLayer Texture;
        public TexturePiece TexturePiece;

        private string previewSuffix = "@preview";
        private string png9Suffix = "@9png";

        internal ImageElement(string name, IPsdLayer layer, ElementType type, IPsdLayer[] childs) : base(name, layer,
            type, childs)
        {
            Texture = layer;
            TexturePiece = new TexturePiece(name, GetTexture2D(layer), false);
        }

        public override void ModifyToPreview(RectTransform root, RectTransform t)
        {
            ModifySize(root, t, Texture);
            Image image = t.gameObject.AddComponent<Image>();
            Texture2D tex = TexturePiece.tex;
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, Texture.Width, Texture.Height), Vector2.zero);
            image.sprite = sprite;
        }

        public override void ModifyToUi(RectTransform root, RectTransform t, string[] sourceDirs)
        {
            ModifySize(root, t, Texture);
            Sprite textureSprite = GetSpriteFromDirectories(TexturePiece.name, sourceDirs);

            var image = t.GetComponent<Image>();
            if (image == null)
                image = t.gameObject.AddComponent<Image>();
            
            image.sprite = textureSprite;
        }

        public override TexturePiece[] GetAllTexturePieces()
        {
            return new[] {
                TexturePiece
            };
        }
    }
}