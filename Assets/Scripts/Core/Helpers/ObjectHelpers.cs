using UnityEngine;

public static class ObjectHelpers
{
    /// <summary>
    /// Recursively traverse up the hierarchy in search of a specific component
    /// </summary>
    public static T RecurseParentsForComponent<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null && go.transform.parent != null)
        {
            component = RecurseParentsForComponent<T>(go.transform.parent.gameObject);
        }
        return component;
    }
    
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
        {
            component = go.AddComponent<T>();
        }
        return component;
    }

    public static GameObject FindOrCreate(GameObject prefab, string name, string id=null, Transform parent=null) {
        if (id != null) {
            name = $"{name}_{id}";
        }
        var go = GameObject.Find(name);
        if (go == null) {
            if (prefab != null) {
                go = GameObject.Instantiate(prefab);
            } else {
                go = new GameObject(name);
            }
            go.name = name;
        }
        if (parent != null) {
            go.transform.SetParent(parent);
        }
        return go;
    }
}