using Microsoft.CodeAnalysis;

namespace Automock.Utils
{
    public static class Extensions
    {
        public static string ToHumanTypeString(this ITypeSymbol typeSymbol)
        {
            return string.Join(string.Empty, typeSymbol.ToDisplayParts(SymbolDisplayFormat.MinimallyQualifiedFormat));
        }
    }
}
