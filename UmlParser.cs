using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static ClassFlow.UmlData;

namespace ClassFlow
{
    internal class UmlParser
    {
        public static UmlData.UmlDiagram Parse(string text)
        {
            var diagram = new UmlData.UmlDiagram();
            var classDict = new Dictionary<string, UmlData.UmlClass>();

            var lines = text.Split(new[] { '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);

            foreach ( var rawLine in lines )
            {
                var line = rawLine.Trim();

                if (line.StartsWith("class "))
                {
                    string className = line.Substring(6).Trim();
                    var umlClass = new UmlData.UmlClass() { Name = className };
                    diagram.Classes.Add(umlClass);
                    classDict[className] = umlClass;
                }
                else if (line.Contains("-->") || line.Contains("<--") || line.Contains("<|--"))
                {
                    string[] parts;
                    string relationType;

                    if (line.Contains("<|--"))
                    {
                        parts = line.Split(new[] { "<|--" }, StringSplitOptions.None);
                        relationType = "<|--";
                    }
                    else if (line.Contains("-->"))
                    {
                        parts = line.Split(new[] { "-->" }, StringSplitOptions.None);
                        relationType = "-->";
                    }
                    else
                    {
                        parts = line.Split(new[] { "<--" }, StringSplitOptions.None);
                        relationType = "<--";
                    }

                    if (parts.Length == 2)
                    {
                        diagram.Relations.Add(new UmlRelation
                        {
                            From = parts[0].Trim(),
                            To = parts[1].Trim(),
                            Type = relationType
                        });
                    }
                }
                else if (Regex.IsMatch(line, @"^[A-Za-z0-9_]+\s*:\s*[+\-#]"))
                {
                    var match = Regex.Match(line, @"^(\w+)\s*:\s*([+\-#])(\w+)\(?\)?\s*:\s*(\w+)$");
                    if (match.Success)
                    {
                        string className = match.Groups[1].Value;
                        string visibility = match.Groups[2].Value;
                        string memberName = match.Groups[3].Value;
                        string memberType = match.Groups[4].Value;

                        var member = new UmlMember
                        {
                            Visibility = visibility,
                            Name = memberName,
                            Type = memberType,
                            IsMethod = line.Contains("()")
                        };

                        if (classDict.ContainsKey(className))
                        {
                            classDict[className].Members.Add(member);
                        }
                    }
                }
            }

            return diagram;
        }
    }
}
