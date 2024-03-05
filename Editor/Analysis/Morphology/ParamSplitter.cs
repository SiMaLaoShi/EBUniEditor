using System;
using System.Collections.Generic;

namespace EBA.Ebunieditor.Editor.Analysis.Morphology
{
    public class ParamSplitter
    {
        public List<(string Type, string Name)> SplitParameters(string parameterString)
        {
            List<(string Type, string Name)> parameters = new List<(string Type, string Name)>();

            // 根据逗号分割参数字符串
            string[] paramDeclarations = parameterString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var declaration in paramDeclarations)
            {
                // 移除额外的空格，然后根据空格进一步拆分类型和名称
                string[] parts = declaration.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    string type = parts[0].Trim();
                    string name = parts[1].Trim();
                    parameters.Add((Type: type, Name: name));
                }
            }

            return parameters;
        }
    }
}