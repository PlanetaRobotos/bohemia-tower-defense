public class Singleton<T> where T : class, new()
{
    public static T Instance { get; private set; }

    protected Singleton() { }

    public static T CreateInstance()
    {
        if (Instance == null)
            Instance = new T();

        return Instance;
    }

    public static bool HasInstance()
    {
        return Instance != null;
    }
}