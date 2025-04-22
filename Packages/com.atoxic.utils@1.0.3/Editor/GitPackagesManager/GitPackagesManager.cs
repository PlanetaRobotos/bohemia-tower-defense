using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Utils.Packages
{
    [Serializable]
    internal struct PackageData
    {
        public string name;
        public string companyName;
        public string displayName;
        public string url;

        internal PackageData(string name, string url, string displayName = null)
        {
            this.name = name;
            this.url = url;

            var splittedName = name.Split(".");

            if (string.IsNullOrEmpty(displayName))
                displayName = splittedName[splittedName.Length - 1];

            this.displayName = displayName;
            companyName = splittedName[1].ToLower();
        }
    }

    internal class PackageVisualInfo
    {
        public PackageData packageData;
        public PackageInfo packageInfo;
    }

    internal class GitPackagesManager : EditorWindow
    {
        private const string UNITY_REGISTRY = "unity";
        private const string GIT_FORMAT = ".git";
        private const string GIT_URL = "https://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@github.com/Galaxy4Games";

        private List<PackageData> _packages = new List<PackageData>();
        private bool _initialized;
        private Vector2 _scrollPos;
        private Dictionary<string, List<PackageVisualInfo>> _visualInfo = new Dictionary<string, List<PackageVisualInfo>>();
        private string _gitUserName;
        private List<string> _fetchedPackages = new List<string>();
        private bool _processing;
        private GitPackagesData _gitPackagesData;

        [MenuItem("Tools/Packages/GitPackagesManager")]
        private static void Init()
        {
            var window = CreateWindow<GitPackagesManager>(nameof(GitPackagesManager));
            var scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(window));
            var mainPackagesFilePath = scriptPath.Replace(nameof(GitPackagesManager) + ".cs", "GitPackagesData.json");
            var mainPackagesJson = File.ReadAllText(mainPackagesFilePath);

            window._gitPackagesData = JsonUtility.FromJson<GitPackagesData>(mainPackagesJson);
            window.minSize = new Vector2(900, 500);

            if (!window._initialized)
                window.Initialize();
        }

        private async void Initialize()
        {
            _initialized = false;

            var listRequest = Client.List();

            while (!listRequest.IsCompleted)
                await Task.Delay(100);

            _packages.Clear();
            _visualInfo.Clear();
            FetchFrom(_gitPackagesData);

            var packagesList = listRequest.Result;

            foreach (var item in packagesList)
            {
                if (!item.name.Contains(UNITY_REGISTRY))
                {
                    AddPackageVisualInfo(item.name, item);
                }
            }

            _initialized = true;
        }

        private void AddPackageVisualInfo(string name, PackageInfo packageInfo)
        {
            var companyName = name.Split(".")[1].ToLower();

            if (!_visualInfo.ContainsKey(companyName))
                _visualInfo.Add(companyName, new List<PackageVisualInfo>());

            var visualInfo = _visualInfo[companyName].Find(x => x.packageData.name == name || x.packageInfo != null && x.packageInfo.name == name);

            if (visualInfo == null)
                _visualInfo[companyName].Add(new PackageVisualInfo() { packageInfo = packageInfo });
            else
                visualInfo.packageInfo = packageInfo;
        }

        private void AddPackageVisualInfo(string name, PackageData packageData)
        {
            var companyName = name.Split(".")[1].ToLower();

            if (!_visualInfo.ContainsKey(companyName))
                _visualInfo.Add(companyName, new List<PackageVisualInfo>());

            var visualInfo = _visualInfo[companyName].Find(x => x.packageData.name == name || x.packageInfo != null && x.packageInfo.name == name);

            if (visualInfo == null)
                _visualInfo[companyName].Add(new PackageVisualInfo() { packageData = packageData });
            else
                visualInfo.packageData = packageData;
        }

        private void OnGUI()
        {
            if (!_initialized)
            {
                EditorGUILayout.LabelField("Initializing...");
                return;
            }

            if (_processing)
            {
                EditorGUILayout.LabelField("Processing...");
                return;
            }

            _gitUserName = EditorGUILayout.TextField("Git User Id:", _gitUserName);

            bool showFetchButton = !string.IsNullOrEmpty(_gitUserName) && !_fetchedPackages.Contains(_gitUserName);

            if (showFetchButton && GUILayout.Button($"Fetch from {_gitUserName}", GUILayout.MinWidth(100), GUILayout.MaxWidth(200)))
                FetchFrom(_gitUserName);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandWidth(true));

            foreach (var item in _visualInfo)
            {
                var author = item.Key;
                var list = item.Value;

                if (list.TrueForAll(x => string.IsNullOrEmpty(x.packageData.name)))
                    continue;

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(author.ToUpper(), new GUIStyle() { fontStyle = FontStyle.Bold });
                EditorGUILayout.Space();

                for (int i = 0; i < list.Count; i++)
                {
                    var visualInfo = list[i];

                    if (string.IsNullOrEmpty(visualInfo.packageData.name))
                        continue;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(visualInfo.packageData.displayName);
                    EditorGUILayout.LabelField(visualInfo.packageInfo == null ? "null" : visualInfo.packageInfo.version);

                    if (visualInfo.packageInfo == null)
                    {
                        if (GUILayout.Button("Install", GUILayout.Width(100)))
                            ProcessRequest(Client.Add(visualInfo.packageData.url));

                        GUILayout.Box("Remove", GUILayout.Width(100));
                    }
                    else
                    {
                        if (GUILayout.Button("Update", GUILayout.Width(100)))
                            ProcessRequest(Client.Add(visualInfo.packageData.url));

                        if (GUILayout.Button("Remove", GUILayout.Width(100)))
                            ProcessRequest(Client.Remove(visualInfo.packageData.name));
                    }

                    if (GUILayout.Button("Link", GUILayout.Width(100)))
                        Application.OpenURL(visualInfo.packageData.url);

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Refresh", GUILayout.Width(100)))
                Initialize();
        }

        private async void ProcessRequest(Request request)
        {
            _initialized = false;

            while (!request.IsCompleted)
                await Task.Delay(100);

            Initialize();
        }

        private async void FetchFrom(GitPackagesData gitPackagesData)
        {
            _processing = true;

            for (int i = 0; i < gitPackagesData.urls.Length; i++)
            {
                var repositoryUserName = gitPackagesData.userName;
                var gitUrl = gitPackagesData.urls[i];
                var gitUrlSplitted = gitUrl.Split("/");
                var repositoryName = gitUrlSplitted[gitUrlSplitted.Length - 1].Replace(".git", string.Empty);
                var packageJsonUrl = $"https://{gitPackagesData.userToken}@raw.githubusercontent.com/{repositoryUserName}/{repositoryName}/HEAD/package.json";
                var packageJsonRequest = UnityWebRequest.Get(packageJsonUrl);

                packageJsonRequest.SendWebRequest();

                while (!packageJsonRequest.isDone)
                    await Task.Delay(10);

                if (packageJsonRequest.result == UnityWebRequest.Result.Success)
                {
                    var packageJson = packageJsonRequest.downloadHandler.text;
                    var packageJsonInfo = JsonUtility.FromJson<PackageJsonInfo>(packageJson);

                    _packages.Add(new PackageData(packageJsonInfo.name, gitUrl, packageJsonInfo.displayName));
                }
            }

            for (int i = 0; i < _packages.Count; i++)
                AddPackageVisualInfo(_packages[i].name, _packages[i]);

            _fetchedPackages.Add(_gitUserName);
            _processing = false;
        }

        private async void FetchFrom(string userId, string tokenId = null)
        {
            _processing = true;

            var url = string.IsNullOrEmpty(tokenId) ? $"https://api.github.com/users/{userId}/repos" : $"https://api.github.com/{userId}/repos";
            var request = UnityWebRequest.Get(url);

            if (!string.IsNullOrEmpty(tokenId))
            {
                request.SetRequestHeader("Accept", "application/vnd.github+json");
                request.SetRequestHeader("Authorization", $"Bearer {tokenId}");
            }

            request.SendWebRequest();

            while (!request.isDone)
                await Task.Delay(100);

            if (request.result == UnityWebRequest.Result.Success)
            {
                var json = request.downloadHandler.text;
                var repositories = Unity.Plastic.Newtonsoft.Json.JsonConvert.DeserializeObject<GitResponseData[]>(json);

                for (int i = 0; i < repositories.Length; i++)
                {
                    var data = repositories[i];
                    var packageJsonUrl = $"https://raw.githubusercontent.com/{userId}/{data.name}/main/package.json";
                    var packageJsonRequest = UnityWebRequest.Get(packageJsonUrl);
                    var gitUrl = data.html_url + GIT_FORMAT;

                    packageJsonRequest.SendWebRequest();

                    while (!packageJsonRequest.isDone)
                        await Task.Delay(10);

                    if (packageJsonRequest.result == UnityWebRequest.Result.Success)
                    {
                        var packageJson = packageJsonRequest.downloadHandler.text;
                        var packageJsonInfo = JsonUtility.FromJson<PackageJsonInfo>(packageJson);

                        _packages.Add(new PackageData(packageJsonInfo.name, gitUrl, packageJsonInfo.displayName));
                    }
                }

                for (int i = 0; i < _packages.Count; i++)
                    AddPackageVisualInfo(_packages[i].name, _packages[i]);

                _fetchedPackages.Add(_gitUserName);
            }

            _processing = false;
        }
    }

    [Serializable]
    internal class GitResponseData
    {
        public string name;
        public string html_url;
    }

    [Serializable]
    internal class GitPackagesData
    {
        public string userName;
        public string userToken;
        public string[] urls;
    }

    [Serializable]
    internal class PackageJsonInfo
    {
        public string name;
        public string displayName;
    }
}