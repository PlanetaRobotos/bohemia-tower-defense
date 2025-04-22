using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Utils.Packages
{
    [InitializeOnLoad]
    internal static class PackagesDependenciesResolver
    {
        private const string UNITY_REGISTRY = "unity";
        private const string GIT_REGISTRY = "git";
        private const string HTTPS_REGISTRY = "https";
        private static bool _resolving;

        static PackagesDependenciesResolver()
        {
            CompilationPipeline.assemblyCompilationFinished += ProcessCompileFinish;
        }

        private static void ProcessCompileFinish(string s, CompilerMessage[] compilerMessages)
        {
            CompilationPipeline.assemblyCompilationFinished -= ProcessCompileFinish;

            bool error = compilerMessages.Count(m => m.type == CompilerMessageType.Error) > 0;

            if (error && !_resolving)
                ResolvePackagesDependencies();
            else
                EditorUtility.ClearProgressBar();
        }

        [MenuItem("Tools/Packages/ResolvePackagesDependencies")]
        public static async void ResolvePackagesDependencies()
        {
            _resolving = true;
            EditorUtility.DisplayProgressBar("Resolve packages dependencies", "Getting packages list", 0f);

            var listRequest = Client.List();

            while (!listRequest.IsCompleted)
                await Task.Delay(100);

            EditorUtility.DisplayProgressBar("Resolve packages dependencies", "Resolving git packages", .5f);

            var packagesList = listRequest.Result;
            var packagesInfos = new List<PackageInfo>();

            foreach (var item in packagesList)
            {
                if (!item.name.Contains(UNITY_REGISTRY))
                    packagesInfos.Add(item);
            }

            var unresolvedPackages = new List<string>();
            var installedPackages = packagesInfos.Select(x => x.name).ToList();

            for (int i = 0; i < packagesInfos.Count; i++)
            {
                var dependenciesInfos = packagesInfos[i].dependencies;

                for (int j = 0; j < dependenciesInfos.Length; j++)
                {
                    var dependencyInfo = dependenciesInfos[j];

                    if (!installedPackages.Contains(dependencyInfo.name) && IsDependencyValid(dependencyInfo) && !unresolvedPackages.Contains(dependencyInfo.version))
                        unresolvedPackages.Add(dependencyInfo.version);
                }
            }

            if (unresolvedPackages.Count > 0)
            {
                for (int i = 0; i < unresolvedPackages.Count; i++)
                {
                    var addRequest = Client.Add(unresolvedPackages[i]);

                    while (!addRequest.IsCompleted)
                        await Task.Delay(100);

                    EditorUtility.DisplayProgressBar("Resolve packages dependencies", "Resolving git packages", .5f + (float)(i + 1) / unresolvedPackages.Count);
                }
            }

            EditorUtility.DisplayProgressBar("Resolve packages dependencies", "Resolving completed.", 1f);
            await Task.Delay(500);
            EditorUtility.ClearProgressBar();
            _resolving = false;
        }

        private static bool IsDependencyValid(DependencyInfo dependencyInfo)
        {
            if (dependencyInfo.name.Contains(UNITY_REGISTRY))
                return false;

            var version = dependencyInfo.version;

            return version.StartsWith(HTTPS_REGISTRY) || version.StartsWith(GIT_REGISTRY);
        }
    }
}