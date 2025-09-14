using System.Text;

namespace XPathReader.Common
{
    public class XPathBuilder : IXPathBuilder
    {
        public readonly StringBuilder _builder = new();

        public int Length
        {
            get { return _builder.Length; }
            set { _builder.Length = value; }
        }

        public void AddLevel(string localName, int index = 0)
        {
            if (index == 0)
            {
                _builder.AppendFormat("/{0}", localName);
            }
            else
            {
                _builder.AppendFormat("/{0}[{1}]", localName, index);
            }
        }

        public string GetXPath()
        {
            return _builder.ToString();
        }

        public override string ToString()
        {
            return GetXPath();
        }
    }
}
