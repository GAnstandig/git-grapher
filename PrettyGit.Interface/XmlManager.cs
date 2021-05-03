using System.Xml.Linq;

namespace PrettyGit.Interface
{
    public static class XmlManager
    {
        public static XDocument GetDocument(string path)
        {
            return XDocument.Load(path);
        }
    }
}
