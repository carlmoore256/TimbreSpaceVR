using UnityEngine;

public static class StringHelpers {

    public static string KebabToCamel(string kebabCase)
    {
        if (string.IsNullOrEmpty(kebabCase))
        {
            return string.Empty;
        }

        string[] words = kebabCase.Split('-');

        // Convert the first word to lower case
        string camelCase = words[0].ToLower();

        // Capitalize the first letter of each subsequent word
        for (int i = 1; i < words.Length; i++)
        {
            string capitalized = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            camelCase += capitalized;
        }

        return camelCase;
    }


    // public static dynamic InferJsonType(string json)
    // {
    //     if (json.StartsWith("{"))
    //     {
    //         return JsonUtility.FromJson<float[]>(json);
    //     }
    //     else if (json.StartsWith("["))
    //     {
    //         return JsonHelper.FromJson(json);
    //     }
    //     else
    //     {
    //         return null;
    //     }
    // }

}