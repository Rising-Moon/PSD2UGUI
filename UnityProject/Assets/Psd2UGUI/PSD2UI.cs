using UnityEngine;
using UnityEngine.UI;
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

        //显示RayCast区域
        public bool drawRaycast = false;
        [HideInInspector] public RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = gameObject.AddComponent<RectTransform>();
        }

        private void OnDrawGizmos()
        {
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

                        //绘制方块
//                        Gizmos.color = new Color(0, 0, 1, 0.2f);
//                        Gizmos.DrawCube((vectors[2] + vectors[0]) / 2, vectors[2] - vectors[0]);
                    }
                }
            }
        }
    }
}