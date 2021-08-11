using System.Xml.Linq;

namespace Interface
{
    public static class XmlManager
    {
        public static XDocument GetDocument(string path)
        {
            return XDocument.Load(path);
        }
    }
}
