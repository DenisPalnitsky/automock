using System;
using Microsoft.CodeAnalysis;

namespace Automock
{
    public class TestClassNameBuilder
    {             
        public static string GetTestClassFileName(
            string classUnderTestName,
            string methodUnderTestName)
        {
            return $"{classUnderTestName}.{methodUnderTestName}.cs";
        }

        public static string GetTestClassFileName(
            string classUnderTestName)
        {
            return $"{classUnderTestName}Tests.cs";
        }

        public static string GetTestClassName(
           string classUnderTestName)
        {
            return $"{classUnderTestName}Tests";
        }   

        internal static string GetNamespaceName(Document targetDocument)
        {            
            return $"{targetDocument.Project.AssemblyName}.{string.Join(".",  targetDocument.Folders)}".TrimEnd('.');
        }
    }
}
