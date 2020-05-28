using SubjectNerd.PsdImporter.PsdParser;

namespace Psd2UGUI.Element.Type
{
    public class SelectBoxElement : PsdElement
    {
        internal SelectBoxElement(string name, IPsdLayer layer, ElementType type, IPsdLayer[] childs) : base(name, layer, type, childs)
        {
        }
        
        
    }
}