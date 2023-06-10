using System.Collections.Generic;

namespace Tidy
{
    [System.Serializable]
    public class RegexConfig
    {
        public string fileType = "";
        public string prefix = "";
        public enum CaseOptionsEnum
        {
            PascalCase = 0,
            CamelCase = 1,
            KebabCase = 2,
            SnakeCase = 3
        }
        public CaseOptionsEnum CaseOptions;

        public string suffix = "";
        public string assetDirectory = "";
    }

    // Wrapper for serializing list
    [System.Serializable]
    public class Wrapper
    {
        public string IgnoredDirectoryPath;
        public List<RegexConfig> List;
    }
}
