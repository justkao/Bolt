using System.IO;

namespace Bolt.Tools
{
    internal static class PathHelpers
    {
        public static string GetOutput(string defaultPath, string userDefinedPath, string defaultFileName)
        {
            if (defaultPath == null)
            {
                defaultPath = Directory.GetCurrentDirectory();
            }

            if (string.IsNullOrEmpty(userDefinedPath))
            {
                return Path.Combine(defaultPath, defaultFileName);
            }

            if (Path.IsPathRooted(userDefinedPath))
            {
                if (Path.HasExtension(userDefinedPath))
                {
                    return userDefinedPath;
                }

                return Path.Combine(userDefinedPath, defaultFileName);
            }

            if (Path.HasExtension(userDefinedPath))
            {
                return Path.Combine(defaultPath, userDefinedPath);
            }

            return Path.Combine(defaultPath, userDefinedPath, defaultFileName);
        }
    }
}
