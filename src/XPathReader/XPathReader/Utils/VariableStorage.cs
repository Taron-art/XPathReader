using System.Text;

namespace XPathReader.Utils
{
    internal class VariableStorage
    {
        private readonly Dictionary<string, string> _variables = new();

        private int _variableIndex = 0;

        public string this[string value]
        {
            get => _variables[value];
        }

        public string Add(string value)
        {
            string newVariable = GetValidVariableName(value);
            if (_variables.ContainsValue(newVariable))
            {
                newVariable += $"_{++_variableIndex}";
            }
            _variables.Add(value, newVariable); ;

            return newVariable;
        }

        private static string GetValidVariableName(string nodeName)
        {
            StringBuilder sb = new(nodeName.Length + 2);
            sb.Append('e');
            sb.Append('_');
            foreach (char c in nodeName)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append('_');
                }
            }

            return sb.ToString();
        }
    }
}
