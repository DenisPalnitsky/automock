namespace Automock
{
    static class TestProjectNameBuilder
    {
        public const string TestProjectNamePattern = "{0}.Test";

        public static string GetProjectName(string currentProjectName)
        {
            return string.Format(TestProjectNamePattern, currentProjectName);
        }
    }
}
