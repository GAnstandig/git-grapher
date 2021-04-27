using System.Xml.Linq;

namespace PrettyGit
{
    public static class XmlManager
    {
        public static XDocument GetDocument(string path)
        {
            return XDocument.Load(path);
        }
    }
}
