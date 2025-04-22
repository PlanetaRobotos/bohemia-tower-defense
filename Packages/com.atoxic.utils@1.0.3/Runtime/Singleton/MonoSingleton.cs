using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    public static T Instance { get; private set; }

    public bool dontDestroyOnLoad;

    public static T CreateInstance(bool dontDestroyOnLoad = false)
    {
        if (Instance == null)
        {
            var instance = new GameObject(typeof(T).Name).AddComponent<T>();
            instance.dontDestroyOnLoad = dontDestroyOnLoad;

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(instance);
        }

        return Instance;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogErrorFormat("Singleton has already instance for the object: {0}", typeof(T).Name);
            Destroy(gameObject);
            return;
        }

        Instance = this as T;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(Instance);

        Init();
    }

    protected virtual void Init()
    {

    }

    public static bool HasInstance => Instance != null;
}