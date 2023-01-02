using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ManagedCoroutine {
    private MonoBehaviour owner;
    private Coroutine coroutine;
    public ManagedCoroutine(MonoBehaviour owner, IEnumerator task, float delay = 0f) {
        this.owner = owner;
        if (delay > 0) {
            coroutine = owner.StartCoroutine(DelayedStart(() => { coroutine = owner.StartCoroutine(task); }, delay));
        } else {
            coroutine = owner.StartCoroutine(task);
        }
    }

    private IEnumerator DelayedStart(Action task, float delay) {
        yield return new WaitForSeconds(delay);
        task.Invoke();
    }

    public void Stop() {
        if (coroutine != null)
            owner.StopCoroutine(coroutine);
    }
}

public class CoroutineManager {

    private MonoBehaviour owner;
    public CoroutineManager(MonoBehaviour owner) {
        this.owner = owner;
    }

    // private Dictionary<string, Dictionary<MonoBehaviour, ManagedCoroutine>> activeCoroutines = new Dictionary<string, Dictionary<MonoBehaviour, ManagedCoroutine>>();
    private Dictionary<string, ManagedCoroutine> activeCoroutines = new Dictionary<string, ManagedCoroutine>();

    public void MoveTowards(Transform transform, Vector3 targetPosition, float duration, Action onStart = null, Action onComplete = null, bool local = false) {
        string id = "MoveTowards";
        if (activeCoroutines.ContainsKey(id))
            activeCoroutines[id].Stop();

        activeCoroutines[id] = new ManagedCoroutine(
            owner, 
            MoveTowardsCoroutine(transform, targetPosition, duration, local, onStart, onComplete, () => {
                // activeCoroutines[id][owner] = null; // maybe do this if memory management isn't working well
                activeCoroutines.Remove(id);
            }), 0f
        );
    }

    /// <summary>
    /// Pass an arbitrary action for MonoBehavior owner to take on start, onupdate, and on complete, over specified duration
    /// </summary>
    public void TimedAction(string id, Action<float> onUpdate, float duration, Action onStart = null, Action onComplete = null, float delay = 0f) {
        if (id == null)
            id = "TimedAction";

        if (activeCoroutines.ContainsKey(id))
            activeCoroutines[id].Stop();

        activeCoroutines[id] = new ManagedCoroutine(
            owner, 
            TimedActionCoroutine(onStart, onUpdate, onComplete, duration, () => { activeCoroutines.Remove(id); }),
            delay
        );
    }


    private IEnumerator TimedActionCoroutine(
            Action onStart, 
            Action<float> onUpdate, 
            Action onComplete, 
            float duration,
            Action onCleanup) {

        onStart?.Invoke();
        float time = 0f;
        while (time < duration) {
            onUpdate?.Invoke(time/duration);
            time += Time.deltaTime;
            yield return null;
        }
        onComplete?.Invoke();
        onCleanup?.Invoke();
    }

    private IEnumerator MoveTowardsCoroutine(
            Transform transform, 
            Vector3 targetPosition, 
            float duration, 
            bool local = false,
            Action onStart = null, 
            Action onComplete = null,
            Action onCleanup = null
            // Action onProgress = null
        ) {
        onStart?.Invoke();
        float time = 0f;        
        while (time < duration) {
            if (local)
                transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, time/duration);
            else
                transform.position = Vector3.Lerp(transform.position, targetPosition, time/duration);
            time += Time.deltaTime;
            yield return null;
        }
        if (local)
            transform.localPosition = targetPosition;
        else
            transform.position = targetPosition;
        onComplete?.Invoke();
        onCleanup?.Invoke();
    }

}



// if (activeCoroutines.ContainsKey(id)) {
//     if (activeCoroutines[id].ContainsKey(owner)) {
//         activeCoroutines[id][owner].Stop();
//     }
// } else {
//     activeCoroutines.Add(id, new Dictionary<MonoBehaviour, ManagedCoroutine>());
// }
// if (activeCoroutines.ContainsKey(id)) {
//     if (activeCoroutines[id].ContainsKey(owner)) {
//         activeCoroutines[id][owner].Stop();
//     }
// } else {
//     activeCoroutines.Add(id, new Dictionary<MonoBehaviour, ManagedCoroutine>());
// }