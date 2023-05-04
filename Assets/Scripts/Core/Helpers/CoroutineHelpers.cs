using UnityEngine;
using System;
using System.Collections;

public static class CoroutineHelpers
{
    public static IEnumerator DelayedAction(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    public static void DelayedAction(Action action, float delay, MonoBehaviour monoBehaviour)
    {
        monoBehaviour.StartCoroutine(DelayedAction(action, delay));
    }

    public static IEnumerator ChangeColor(object obj, Color startColor, Color endColor, float duration)
    {
        float startTime = Time.time;
        float endTime = startTime + duration;
        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / duration;
            Color newColor = Color.Lerp(startColor, endColor, t);
            if (obj == null)
            {
                yield break;
            }
            obj.GetType().GetProperty("color").SetValue(obj, newColor, null);
            yield return null;
        }
    }

    public static void ChangeColor(object obj, Color startColor, Color endColor, float duration, MonoBehaviour monoBehaviour)
    {
        monoBehaviour.StartCoroutine(ChangeColor(obj, startColor, endColor, duration));
    }
}