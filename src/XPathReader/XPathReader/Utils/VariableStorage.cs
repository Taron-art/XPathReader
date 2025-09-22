using System.Text;

namespace XPathReader.Utils
{
    internal class VariableStorage
    {
        private readonly Dictionary<string, string> _variables = new();

        private int _variableIndex = 0;
        private StringBuilder? _variableNameBuilder;

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

        private string GetValidVariableName(string nodeName)
        {
            StringBuilder stringBuilder = (_variableNameBuilder ??= new(nodeName.Length + 2));
            stringBuilder.Length = 0;
            stringBuilder.Append('e');
            stringBuilder.Append('_');
            foreach (char c in nodeName)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    stringBuilder.Append(c);
                }
                else
                {
                    stringBuilder.Append('_');
                }
            }

            return stringBuilder.ToString();
        }
    }
}
