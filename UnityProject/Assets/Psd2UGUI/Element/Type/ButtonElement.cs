using Psd2UGUI.Utils;
using SubjectNerd.PsdImporter.PsdParser;
using UnityEngine;
using UnityEngine.UI;

namespace Psd2UGUI.Element.Type
{
    public class ButtonElement : PsdElement
    {
        public IPsdLayer normal;
        public IPsdLayer pressed;
        public IPsdLayer disabled;

        public TexturePiece normalPiece;
        public TexturePiece pressedPiece;
        public TexturePiece disabledPiece;

        //初始化
        internal ButtonElement(string name, IPsdLayer layer, ElementType type, IPsdLayer[] childs) : base(name, layer,
            type, childs)
        {
            var normalSuffix = "@normal";
            var pressedSuffix = "@pressed";
            var disabledSuffix = "@disabled";

            normal = FindChildElement(normalSuffix);
            if (normal != null && normal.HasImage)
                normalPiece = new TexturePiece(name + normalSuffix, GetTexture2D(normal));
            else
            {
                canShow = false;
                P2UUtil.ShowError("按钮:" + name + "至少要有一张正常态的图片");
            }

            pressed = FindChildElement(pressedSuffix);
            if (pressed != null && pressed.HasImage)
                pressedPiece = new TexturePiece(name + pressedSuffix, GetTexture2D(pressed));

            disabled = FindChildElement(disabledSuffix);
            if (disabled != null && disabled.HasImage)
                disabledPiece = new TexturePiece(name + disabledSuffix, GetTexture2D(disabled));
        }

        //获取所有可导出图片
        public override TexturePiece[] GetAllTexturePieces()
        {
            return new[] {normalPiece, pressedPiece, disabledPiece};
        }

        //生成Preview
        public override void ModifyToPreview(RectTransform root, RectTransform t)
        {
            if (!canShow)
                return;

            ModifySize(root, t, this.normal);
            Rect btnRect = new Rect(0, 0, this.normal.Width, this.normal.Height);

            Image image = t.gameObject.AddComponent<Image>();
            Button button = t.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Sprite normal = Sprite.Create(normalPiece.tex, btnRect, Vector2.zero);
            image.sprite = normal;

            if (pressed != null || disabled != null)
            {
                button.transition = Selectable.Transition.SpriteSwap;
                Sprite pressed = null;
                Sprite disabled = null;
                if (this.pressed != null)
                    pressed = Sprite.Create(pressedPiece.tex, btnRect, Vector2.zero);
                if (this.disabled != null)
                    disabled = Sprite.Create(disabledPiece.tex, btnRect, Vector2.zero);
                SpriteState state = new SpriteState();
                state.pressedSprite = pressed;
                state.disabledSprite = disabled;
                button.spriteState = state;
            }
        }

        //生成UI
        public override void ModifyToUi(RectTransform root, RectTransform t, string[] sourceDirs)
        {
            if (!canShow)
                return;

            ModifySize(root, t, normal);

            Button button = t.GetComponent<Button>();
            if (button == null)
            {
                button = t.gameObject.AddComponent<Button>();
                var image = t.gameObject.AddComponent<Image>();
                button.targetGraphic = image;
            }

            Sprite normalSprite = GetSpriteFromDirectories(normalPiece.name, sourceDirs);
            Sprite pressedSprite = null;
            if (pressed != null)
                pressedSprite = GetSpriteFromDirectories(pressedPiece.name, sourceDirs);
            Sprite disabledSprite = null;
            if (disabled != null)
                disabledSprite = GetSpriteFromDirectories(disabledPiece.name, sourceDirs);

            SpriteState state = new SpriteState();
            state.pressedSprite = pressedSprite;
            state.disabledSprite = disabledSprite;

            button.image.sprite = normalSprite;
            button.spriteState = state;
        }
    }
}