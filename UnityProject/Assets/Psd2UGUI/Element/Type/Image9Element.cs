using SubjectNerd.PsdImporter.PsdParser;
using UnityEngine;
using UnityEngine.UI;

namespace Psd2UGUI.Element.Type
{
    public class Image9Element : PsdElement
    {
        public IPsdLayer Png9;
        public TexturePiece Png9Piece;
        private string png9Suffix = "@png9";

        public IPsdLayer Preview;
        public TexturePiece PreviewPiece;
        private string previewSuffix = "@preview";

        public Image9Element(string name, IPsdLayer layer, ElementType type, IPsdLayer[] childs) : base(name, layer,
            type, childs)
        {
            Png9 = FindChildElement(png9Suffix);
            if (Png9 != null)
                Png9Piece = new TexturePiece(name, GetTexture2D(Png9), true);

            Preview = FindChildElement(previewSuffix);
            if (Preview != null)
                PreviewPiece = new TexturePiece(name + previewSuffix, GetTexture2D(Preview), false);
        }

        public override void ModifyToPreview(RectTransform root, RectTransform t)
        {
            ModifySize(root, t, Preview);
            Image image = t.gameObject.AddComponent<Image>();
            Texture2D tex = PreviewPiece.tex;
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, Preview.Width, Preview.Height), Vector2.zero);
            image.sprite = sprite;
        }

        public override void ModifyToUi(RectTransform root, RectTransform t, string[] sourceDirs)
        {
            ModifySize(root, t, Preview);
            Image image = t.GetComponent<Image>();
            Sprite image9Sprite = GetSpriteFromDirectories(Png9Piece.name, sourceDirs);

            if (image == null)
                image = t.gameObject.AddComponent<Image>();

            image.type = Image.Type.Sliced;
            image.sprite = image9Sprite;
        }

        //导出图片
        public override TexturePiece[] GetAllTexturePieces()
        {
            return new[] {
                Png9Piece
            };
        }
    }
}