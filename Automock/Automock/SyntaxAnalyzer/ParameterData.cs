using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automock.SyntaxAnalyzer
{
    class ParameterData
    {
        public ParameterData(ITypeSymbol typeSymbol, string name)
        {
            TypeSymbol = typeSymbol;
            Name = name;
        }

        public ITypeSymbol TypeSymbol { get; private set; }
        public string Name { get; private set; }
    }
}
