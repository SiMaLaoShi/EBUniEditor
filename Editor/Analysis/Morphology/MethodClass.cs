using System.Collections.Generic;

namespace EBA.Ebunieditor.Editor.Analysis.Morphology
{
    public class MethodClass
    {
        public string AccessModifier { get; set; }
        public string ReturnType { get; set; }
        public string MethodName { get; set; }
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();

        public class Parameter
        {
            public string Type { get; set; }
            public string Name { get; set; }
        }
    }
}