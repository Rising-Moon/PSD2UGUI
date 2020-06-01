using Psd2UGUI.Utils;
using SubjectNerd.PsdImporter.PsdParser;
using UnityEngine;
using UnityEngine.UI;

namespace Psd2UGUI.Element.Type
{
    public class SelectBoxElement : PsdElement
    {
        public IPsdLayer normal;
        public TexturePiece normalPiece;
        private string normalSuffix = "@normal";

        public IPsdLayer select;
        public TexturePiece selectPiece;
        private string selectSuffix = "@select";

        internal SelectBoxElement(string name, IPsdLayer layer, ElementType type, IPsdLayer[] childs) : base(name,
            layer, type, childs)
        {
            normal = FindChildElement(normalSuffix);
            if (normal != null)
                normalPiece = new TexturePiece(name + normalSuffix, GetTexture2D(normal));
            else
            {
                canShow = false;
                P2UUtil.AddError("勾选框:" + name + "需要有一张未选择态的图");
            }

            select = FindChildElement(selectSuffix);
            if (select != null)
                selectPiece = new TexturePiece(name + selectSuffix, GetTexture2D(select));
            else
            {
                canShow = false;
                P2UUtil.AddError("勾选框:" + name + "需要有一张选择态的图");
            }
        }

        public override void ModifyToPreview(RectTransform root, RectTransform t)
        {
            ModifySize(root, t, normal);

            var normalGameObject = CreateGameObject("normal", t).GetComponent<RectTransform>();
            ModifySize(root, normalGameObject, normal);
            var normalImage = normalGameObject.gameObject.AddComponent<Image>();
            normalImage.sprite =
                Sprite.Create(normalPiece.tex, new Rect(0, 0, normal.Width, normal.Height), Vector2.zero);

            var selectGameObject = CreateGameObject("select", t).GetComponent<RectTransform>();
            ModifySize(root, selectGameObject, normal);
            var selectImage = selectGameObject.gameObject.AddComponent<Image>();
            selectImage.sprite =
                Sprite.Create(selectPiece.tex, new Rect(0, 0, @select.Width, @select.Height), Vector2.zero);

            var toggle = t.gameObject.AddComponent<Toggle>();
            toggle.transition = Selectable.Transition.None;
            toggle.toggleTransition = Toggle.ToggleTransition.None;
            toggle.graphic = selectImage;
            toggle.targetGraphic = normalImage;
        }

        public override void ModifyToUi(RectTransform root, RectTransform t, string[] sourceDirs)
        {
            ModifySize(root, t, normal);

            var normalRectTransform = t.Find("normal") as RectTransform;
            if (normalRectTransform == null)
            {
                normalRectTransform = CreateGameObject("normal", t).GetComponent<RectTransform>();
                normalRectTransform.gameObject.AddComponent<Image>();
            }

            var normalImage = normalRectTransform.GetComponent<Image>();
            normalImage.sprite = GetSpriteFromDirectories(normalPiece.name, sourceDirs);
            ModifySize(root, normalRectTransform, select);


            var selectRectTransform = t.Find("select") as RectTransform;
            if (selectRectTransform == null)
            {
                selectRectTransform = CreateGameObject("select", t).GetComponent<RectTransform>();
                selectRectTransform.gameObject.AddComponent<Image>();
            }

            var selectImage = selectRectTransform.GetComponent<Image>();
            selectImage.sprite = GetSpriteFromDirectories(selectPiece.name, sourceDirs);
            ModifySize(root, selectRectTransform, normal);

            var toggle = t.GetComponent<Toggle>();
            if (toggle == null)
            {
                toggle = t.gameObject.AddComponent<Toggle>();
                toggle.transition = Selectable.Transition.None;
                toggle.toggleTransition = Toggle.ToggleTransition.None;
                toggle.graphic = selectImage;
                toggle.targetGraphic = normalImage;
            }
        }

        public override TexturePiece[] GetAllTexturePieces()
        {
            return new[] {normalPiece, selectPiece};
        }
    }
}