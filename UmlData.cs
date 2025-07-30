using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassFlow
{
    internal class UmlData
    {
        public class UmlClass
        {
            public string Name { get; set; }
            public List<UmlMember> Members { get; set; } = new List<UmlMember>();
        }

        public class UmlMember
        {
            public string Visibility { get; set; } // +, -, #
            public string Name { get; set; }
            public string Type { get; set; }
            public bool IsMethod { get; set; }
        }

        public class UmlRelation
        {
            public string From { get; set; }
            public string To { get; set; }
            public string Type { get; set; } // -->, <|-- и т.д.
        }

        public class UmlDiagram
        {
            public List<UmlClass> Classes { get; set; } = new List<UmlClass>();
            public List<UmlRelation> Relations { get; set; } = new List<UmlRelation>();
        }

    }
}
