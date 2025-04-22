using System.IO;
using UnityEditor;
using UnityEngine;

namespace Utils
{
    public class Tools
    {
        [MenuItem("Tools/Packages/Resolve Packages")]
        public static void ResolvePackages()
        {
            var filePath = Application.dataPath.Replace("Assets", "Packages") + "/packages-lock.json";
            FileUtil.DeleteFileOrDirectory(filePath);
            UnityEditor.PackageManager.Client.Resolve();
        }

        [MenuItem("Tools/Remove Visual Studio Solutions")]
        public static void RemoveVisualStudioSolutions()
        {
            var projectDirectoryPath = Application.dataPath.Replace("/Assets", string.Empty);

            var directory = new DirectoryInfo(projectDirectoryPath);
            var files = directory.GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];

                if (file.FullName.EndsWith(".csproj") || file.FullName.EndsWith(".sln"))
                    FileUtil.DeleteFileOrDirectory(file.FullName);
            }

            FileUtil.DeleteFileOrDirectory(projectDirectoryPath + "/.vs");
        }
    }
}