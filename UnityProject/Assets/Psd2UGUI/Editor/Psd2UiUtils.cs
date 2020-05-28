using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Psd2UGUI.Editor
{
    public class Psd2UiUtils : UnityEditor.Editor
    {
        [MenuItem("GameObject/UI/PSD2UGUI Object")]
        public static void CreateGameObject()
        {
            GameObject canvasObject = GameObject.Find("Canvas");
            if (canvasObject == null)
            {
                canvasObject = new GameObject("Canvas");
                var rectTransform = canvasObject.AddComponent<RectTransform>();
                var canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = Camera.main;
                var canvasScaler = canvasObject.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.referenceResolution = new Vector2(1280, 720);
                var graphicRaycaster = canvasObject.AddComponent<GraphicRaycaster>();
            }

            GameObject eventSystemObject = GameObject.Find("EventSystem");
            if (eventSystemObject == null)
            {
                eventSystemObject = new GameObject("EventSystem");
                var eventSystem = eventSystemObject.AddComponent<EventSystem>();
                var standaloneInputModule = eventSystemObject.AddComponent<StandaloneInputModule>();
            }

            GameObject gameObject = new GameObject();
            gameObject.transform.SetParent(canvasObject.transform);
            gameObject.AddComponent<PSD2UI>();
        }
    }
}