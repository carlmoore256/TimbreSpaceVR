using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class JsonHelper
{
    public static string fixJson(string value)
    {
        value = "{\"Items\":" + value + "}";
        return value;
    }

    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }

    public static T AtomicJsonToObject<T>(string json)
    {
        // Deserialize the JSON to a generic object
        object genericObject = JsonUtility.FromJson<object>(json);

        // If the generic object is a dictionary, convert it to a strongly-typed object
        if (genericObject is Dictionary<string, object> genericDict)
        {
            // Get the type of the target object
            Type targetType = typeof(T);

            // Create a new instance of the target object
            T targetObject = Activator.CreateInstance<T>();

            // Loop through the properties of the target object
            foreach (var property in targetType.GetProperties())
            {
                // Check if the dictionary contains a key that matches the property name
                if (genericDict.TryGetValue(StringHelpers.KebabToCamel(property.Name), out object value))
                {
                    // Convert the value to the appropriate type and set the property value
                    object typedValue = Convert.ChangeType(value, property.PropertyType);
                    property.SetValue(targetObject, typedValue);
                }
            }

            return targetObject;
        }

        return default(T);
    }
}