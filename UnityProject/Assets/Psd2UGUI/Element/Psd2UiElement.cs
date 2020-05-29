using Psd2UGUI.Element.Type;
using UnityEngine;

namespace Psd2UGUI.Element
{
    /// <summary>
    /// 挂载在GameObject上用于标示生成的UI
    /// </summary>
    public class Psd2UiElement : MonoBehaviour
    {
        public PsdElement.ElementType type;
        public bool linkPsd = true;
    }
}