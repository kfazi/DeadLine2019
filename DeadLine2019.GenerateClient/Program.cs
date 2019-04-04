namespace DeadLine2019.GenerateClient
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public class Program
    {
        public static void Main(string[] args)
        {
            var fileLines = File.ReadAllLines(@"Commands.txt");

            var regex = new Regex(
                @"(?<method>\w+)(,?\s*?((?<parameter>\w+):\s*?(?<parameter_type>[\w<>]+)))*\s*?->\s*?(?<result_type>[\w<>]+)", RegexOptions.ExplicitCapture);

            foreach (var fileLine in fileLines)
            {
                if (string.IsNullOrWhiteSpace(fileLine))
                {
                    continue;
                }

                var builder = new StringBuilder();

                var match = regex.Match(fileLine);
                if (!match.Success)
                {
                    continue;
                }

                var parameters = new List<string>();
                for (var i = 0; i < match.Groups["parameter"].Captures.Count; i++)
                {
                    var name = match.Groups["parameter"].Captures[i];
                    var type = match.Groups["parameter_type"].Captures[i];
                    parameters.Add($"{type} {name}");
                }

                builder.AppendLine($"public {match.Groups["result_type"]} {match.Groups["method"]}({string.Join(", ", parameters)})");
                builder.AppendLine("{");
                builder.AppendLine($"    {SendCommand(match.Groups["method"].Value, match.Groups["parameter"].Captures.Cast<Capture>())}");
                builder.AppendLine("    CheckResponse(NetworkClient.ReadLine());");
                if (match.Groups["result_type"].Value != "void")
                {
                    builder.AppendLine("    var tokenReader = GetTokenReader();");
                }

                switch (match.Groups["result_type"].Value)
                {
                    case "void":
                        break;
                    case "int":
                        builder.AppendLine("    return tokenReader.ReadInt();");
                        break;
                    case "uint":
                        builder.AppendLine("    return tokenReader.ReadUInt();");
                        break;
                    case "float":
                        builder.AppendLine("    return (float)tokenReader.ReadDouble();");
                        break;
                    case "double":
                        builder.AppendLine("    return tokenReader.ReadDouble();");
                        break;
                    case "string":
                        builder.AppendLine("    return tokenReader.ReadString();");
                        break;
                    default:
                        builder.AppendLine("    throw new NotImplementedException();");
                        break;
                }
                builder.AppendLine("}");

                Console.WriteLine(builder.ToString());
            }

            Console.ReadKey();
        }

        public static string SendCommand(string method, IEnumerable<Capture> parameters)
        {
            if (parameters.Any())
            {
                return $"NetworkClient.SendLine($\"{GetMethodName(method)} {string.Join(", ", parameters.Select(x => $"{{{x}}}"))}\");";
            }

            return $"NetworkClient.SendLine($\"{GetMethodName(method)}\");";
        }

        public static string GetMethodName(string method)
        {
            return Regex.Replace(method, "(\\B[A-Z])", "_$1").ToUpperInvariant();
        }
    }
}
