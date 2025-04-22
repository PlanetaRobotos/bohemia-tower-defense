using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.Text;
using System.Linq;

public class FindMissingUpdaterSubscriptions : EditorWindow
{
    private Vector2 scrollPosition;
    private List<string> filesWithOnUpdate = new List<string>();
    private List<string> filesWithOverriddenOnUpdate = new List<string>();
    private List<string> filesMissingSubscription = new List<string>();
    private List<string> filesWithUnusedOnUpdate = new List<string>();
    private Dictionary<string, string> fileContents = new Dictionary<string, string>();

    [MenuItem("Tools/Find Missing Updater Subscriptions")]
    public static void ShowWindow()
    {
        GetWindow<FindMissingUpdaterSubscriptions>("Missing Updater Subscriptions");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        
        GUILayout.Label("Find files with OnUpdate but missing IUpdater subscription", EditorStyles.boldLabel);

        if (GUILayout.Button("Scan Project"))
        {
            ScanProject();
        }

        if (filesMissingSubscription.Count > 0)
        {
            if (GUILayout.Button("Add Missing Subscriptions"))
            {
                AddMissingSubscriptions();
            }
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField($"Base OnUpdate methods: {filesWithOnUpdate.Count}", EditorStyles.boldLabel);
        foreach (var file in filesWithOnUpdate)
        {
            EditorGUILayout.BeginHorizontal();
            string label = file;
            if (filesWithUnusedOnUpdate.Contains(file))
            {
                label += " (UNUSED)";
                GUI.color = Color.yellow;
            }
            EditorGUILayout.LabelField(label);
            GUI.color = Color.white;
            
            if (filesMissingSubscription.Contains(file))
            {
                if (GUILayout.Button("Fix", GUILayout.Width(50)))
                {
                    AddSubscriptionToFile(file);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Overridden OnUpdate methods: {filesWithOverriddenOnUpdate.Count}", EditorStyles.boldLabel);
        foreach (var file in filesWithOverriddenOnUpdate)
        {
            EditorGUILayout.LabelField(file);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Files missing subscription: {filesMissingSubscription.Count}", EditorStyles.boldLabel);
        foreach (var file in filesMissingSubscription)
        {
            EditorGUILayout.LabelField(file);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Files with unused OnUpdate: {filesWithUnusedOnUpdate.Count}", EditorStyles.boldLabel);
        foreach (var file in filesWithUnusedOnUpdate)
        {
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField(file);
            GUI.color = Color.white;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void ScanProject()
    {
        filesWithOnUpdate.Clear();
        filesWithOverriddenOnUpdate.Clear();
        filesMissingSubscription.Clear();
        filesWithUnusedOnUpdate.Clear();
        fileContents.Clear();

        string[] allScripts = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

        // First pass: collect all files and their contents
        foreach (string scriptPath in allScripts)
        {
            try
            {
                string relativePath = scriptPath.Replace(Application.dataPath, "Assets");
                string content = File.ReadAllText(scriptPath);
                fileContents[relativePath] = content;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error reading file {scriptPath}: {e.Message}");
            }
        }

        // Second pass: analyze OnUpdate methods and their usage
        foreach (var filePair in fileContents)
        {
            string relativePath = filePair.Key;
            string content = filePair.Value;

            try
            {
                // Check for overridden OnUpdate methods
                if (Regex.IsMatch(content, @"(?:protected|private)\s+override\s+(?:virtual\s+)?void\s+OnUpdate\s*\("))
                {
                    filesWithOverriddenOnUpdate.Add(relativePath);
                    continue; // Skip override methods for unused check
                }

                // Check for base OnUpdate methods
                if (Regex.IsMatch(content, @"(?<!override\s+)(?:protected|private)\s+(?:virtual\s+)?void\s+OnUpdate\s*\("))
                {
                    // Skip if the class is abstract
                    if (!Regex.IsMatch(content, @"abstract\s+class"))
                    {
                        filesWithOnUpdate.Add(relativePath);

                        // Extract class name to check for usage
                        string className = ExtractClassName(content);
                        bool isUsed = false;

                        if (!string.IsNullOrEmpty(className))
                        {
                            // Check if OnUpdate is subscribed or called in any file
                            foreach (var otherContent in fileContents.Values)
                            {
                                if (Regex.IsMatch(otherContent, $@"Updater\.Subscribe\(\s*{className}\.OnUpdate\b") ||
                                    Regex.IsMatch(otherContent, $@"Updater\.Subscribe\(\s*OnUpdate\b") ||
                                    Regex.IsMatch(otherContent, $@"\b{className}\.OnUpdate\s*\(") ||
                                    Regex.IsMatch(otherContent, @"base\.OnUpdate\s*\("))
                                {
                                    isUsed = true;
                                    break;
                                }
                            }
                        }

                        if (!isUsed)
                        {
                            filesWithUnusedOnUpdate.Add(relativePath);
                        }

                        // Check if file is missing IUpdater subscription
                        if (!Regex.IsMatch(content, @"Updater\.Subscribe\(OnUpdate"))
                        {
                            filesMissingSubscription.Add(relativePath);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error analyzing file {relativePath}: {e.Message}");
            }
        }

        Debug.Log($"Total OnUpdate methods found: {filesWithOnUpdate.Count + filesWithOverriddenOnUpdate.Count}");
        Debug.Log($"Unused OnUpdate methods found: {filesWithUnusedOnUpdate.Count}");
    }

    private string ExtractClassName(string content)
    {
        var match = Regex.Match(content, @"class\s+(\w+)");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private void AddMissingSubscriptions()
    {
        foreach (var file in new List<string>(filesMissingSubscription))
        {
            AddSubscriptionToFile(file);
        }
        ScanProject(); // Refresh the lists after modifications
    }

    private void AddSubscriptionToFile(string filePath)
    {
        try
        {
            if (!fileContents.ContainsKey(filePath))
            {
                Debug.LogError($"File {filePath} was not scanned. Please run Scan Project first.");
                return;
            }

            string content = fileContents[filePath];
            string[] lines = content.Split('\n');
            List<string> newLines = new List<string>();
            bool hasUsingInfrastructure = false;
            bool hasUpdaterProperty = false;
            bool hasAwakeMethod = false;
            bool hasOnDestroyMethod = false;
            int classIndentation = 0;
            int currentIndentation = 0;

            // First pass: analyze the file
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Contains("using Infrastructure.Services.ApplicationObservers.Runtime"))
                {
                    hasUsingInfrastructure = true;
                }
                if (line.Contains("[Inject] private IUpdater Updater"))
                {
                    hasUpdaterProperty = true;
                }
                if (line.Contains("void Awake()"))
                {
                    hasAwakeMethod = true;
                }
                if (line.Contains("void OnDestroy()"))
                {
                    hasOnDestroyMethod = true;
                }
                if (line.Contains("class"))
                {
                    classIndentation = line.TakeWhile(c => char.IsWhiteSpace(c)).Count();
                }
            }

            // Second pass: modify the file
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                currentIndentation = line.TakeWhile(c => char.IsWhiteSpace(c)).Count();

                // Add using statement at the top with other usings
                if (!hasUsingInfrastructure && line.TrimStart().StartsWith("using") && !lines[i - 1].TrimStart().StartsWith("using"))
                {
                    newLines.Add("using Infrastructure.Services.ApplicationObservers.Runtime;");
                }

                newLines.Add(line);

                // Add Updater property after class declaration
                if (!hasUpdaterProperty && line.Contains("class") && line.Contains("{"))
                {
                    string indent = new string(' ', currentIndentation + 4);
                    newLines.Add($"{indent}[Inject] private IUpdater Updater {{ get; }}");
                }

                // Add subscription to existing Awake method
                if (!line.Contains("Updater.Subscribe") && line.Contains("void Awake()") && line.Contains("{"))
                {
                    string indent = new string(' ', currentIndentation + 4);
                    newLines.Add($"{indent}Updater.Subscribe(OnUpdate, 0);");
                }

                // Add unsubscription to existing OnDestroy method
                if (!line.Contains("Updater?.Unsubscribe") && line.Contains("void OnDestroy()") && line.Contains("{"))
                {
                    string indent = new string(' ', currentIndentation + 4);
                    newLines.Add($"{indent}Updater?.Unsubscribe(OnUpdate);");
                }
            }

            // Add missing methods at the end of the class if needed
            if (!hasAwakeMethod || !hasOnDestroyMethod)
            {
                for (int i = newLines.Count - 1; i >= 0; i--)
                {
                    if (newLines[i].TrimEnd() == "}")
                    {
                        string indent = new string(' ', classIndentation + 4);
                        
                        if (!hasAwakeMethod)
                        {
                            newLines.Insert(i, $"{indent}}}");
                            newLines.Insert(i, $"{indent}    Updater.Subscribe(OnUpdate, 0);");
                            newLines.Insert(i, $"{indent}{{");
                            newLines.Insert(i, $"{indent}protected virtual void Awake()");
                            newLines.Insert(i, "");
                        }
                        
                        if (!hasOnDestroyMethod)
                        {
                            newLines.Insert(i, $"{indent}}}");
                            newLines.Insert(i, $"{indent}    Updater?.Unsubscribe(OnUpdate);");
                            newLines.Insert(i, $"{indent}{{");
                            newLines.Insert(i, $"{indent}protected virtual void OnDestroy()");
                            newLines.Insert(i, "");
                        }
                        
                        break;
                    }
                }
            }

            string fullPath = filePath.Replace("Assets", Application.dataPath);
            File.WriteAllText(fullPath, string.Join("\n", newLines));
            AssetDatabase.Refresh();
            
            Debug.Log($"Successfully updated {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating file {filePath}: {e.Message}");
        }
    }
} 