using UnityEngine;

public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    private static T _instance = null;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                T[] instances = Resources.FindObjectsOfTypeAll<T>();
                if (instances.Length == 0)
                {
                    Debug.LogError("No Singleton of type " + typeof(T).ToString() + " found in resources");
                    return null;
                } if (instances.Length > 1)
                {
                    Debug.LogError("Multiple Singletons of type " + typeof(T).ToString() + " found in resources");
                    return null;
                }
                
                _instance = instances[0];
                _instance.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            return _instance;
        }
    }
}
