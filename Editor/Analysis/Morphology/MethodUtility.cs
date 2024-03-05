using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EBA.Ebunieditor.Editor.Analysis.Morphology
{
    public class MethodUtility
    {
        public static void ReadDllImportWriteCS(string fileName)
        {
            var lines = File.ReadAllLines(fileName);
            var methods = new List<MethodClass>();

            foreach (var line in lines)
            {
                var method = Parse(line);
                if (!string.IsNullOrEmpty(method.MethodName))
                    methods.Add(method);
            }

            // 现在 methods 列表包含了从文件中解析出的所有方法
            // 你现在可以处理这些方法，例如打印它们或进行其他逻辑处理。
            var sb = new StringBuilder();
            sb.AppendLine("#region 函数体");
            foreach (var method in methods)
            {
                Debug.Log(method);
                var parameters = string.Join(", ", method.Parameters.ConvertAll(p => $"{p.Type} {p.Name}"));
                sb.AppendLine($"public static {method.ReturnType} CS_{method.MethodName}({parameters})");
                sb.AppendLine("{");
                var invokeParameters = string.Join(", ", method.Parameters.ConvertAll(p => $"{p.Name} "));
                sb.AppendLine("\t #if UNITY_IPHONE");
                if (method.ReturnType == "void")
                    sb.AppendLine($"\t{method.MethodName}({invokeParameters});");
                else
                    sb.AppendLine($"\t return {method.MethodName}({invokeParameters});");
                sb.AppendLine("\t #endif");
                if (method.ReturnType == "string")
                    sb.AppendLine($"\t return string.Empty;");
                else if (method.ReturnType == "int")
                    sb.AppendLine($"\t return 0;");
                sb.AppendLine("}");
            }

            sb.AppendLine("#endregion 函数体");
            File.WriteAllText(Path.Combine(System.Environment.CurrentDirectory, "AllMethod.txt"), sb.ToString());
            Debug.Log(sb);
        }
        
        public static void ReadMMUnitySendMessage(string fileName)
        {
            var lines = File.ReadAllLines(fileName);
            var listMethod = new List<string>();
            foreach (var line in lines)
            {
                var m = ExtractSecondParameter(line);
                if (null != m && !listMethod.Contains(m))
                {
                    listMethod.Add(m);
                    Debug.Log(m);
                }
            }

            var sb = new StringBuilder();
            foreach (var m in listMethod)
            {
                sb.AppendLine($"public void {m}(string str) ");
                sb.AppendLine("{");
                sb.AppendLine("}");
            }
            Debug.Log(sb);
        }
        
        public static string ExtractSecondParameter(string unitySendMessageCall)
        {
            // 正则表达式用于匹配UnitySendMessage调用，并获取第二个参数
            var regexPattern = @"UnitySendMessage\(\s*""[^""]+"",\s*""([^""]+)"",\s*[^)]+\s*\)";
            var match = Regex.Match(unitySendMessageCall, regexPattern);

            if (match.Success && match.Groups.Count > 1)
            {
                // 返回第二个参数
                return match.Groups[1].Value;
            }

            return null; // 如果没有找到匹配，返回null
        }

        public static MethodClass Parse(string methodSignature)
        {
            var externMethod = new MethodClass();

            // Parse the method signature using regular expressions
            var regex = new Regex(@"(private|public|protected|internal)\s+static\s+extern\s+(\w+)\s+(\w+)\(([^)]*)\)");
            var match = regex.Match(methodSignature);

            if (match.Success)
            {
                externMethod.AccessModifier = match.Groups[1].Value;
                externMethod.ReturnType = match.Groups[2].Value;
                externMethod.MethodName = match.Groups[3].Value;

                var paramsRegex = new Regex(@"(\w+)\s+(\w+)");
                var paramsMatch = paramsRegex.Matches(match.Groups[4].Value);

                foreach (Match paramMatch in paramsMatch)
                {
                    externMethod.Parameters.Add(new MethodClass.Parameter
                    {
                        Type = paramMatch.Groups[1].Value,
                        Name = paramMatch.Groups[2].Value
                    });
                }
            }

            return externMethod;
        }
    }
}