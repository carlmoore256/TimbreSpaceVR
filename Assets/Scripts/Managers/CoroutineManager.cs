using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct TargetTransform {
    public Vector3 targetPosition;
    public Quaternion targetRotation;
    public Vector3 targetScale;
    public TargetTransform(Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale) {
        this.targetPosition = targetPosition;
        this.targetRotation = targetRotation;
        this.targetScale = targetScale;
    }
}

public class ManagedCoroutine {
    private MonoBehaviour owner;
    private Coroutine coroutine;
    public float Duration { get; private set; }
    public ManagedCoroutine(MonoBehaviour owner, IEnumerator task, float duration, float delay = 0f) {
        this.owner = owner;
        this.Duration = duration;
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

    protected MonoBehaviour owner;
    protected Dictionary<string, ManagedCoroutine> activeCoroutines = new Dictionary<string, ManagedCoroutine>();
    public CoroutineManager(MonoBehaviour owner) {
        this.owner = owner;
    }

    public void StopAllCoroutines() {
        foreach (var coroutine in activeCoroutines.Values)
            coroutine.Stop();
        activeCoroutines.Clear();
    }

    private void AddCoroutine(string id, ManagedCoroutine coroutine) {
        if (!activeCoroutines.ContainsKey(id))
            activeCoroutines[id] = coroutine;
        else {
            activeCoroutines[id].Stop();
            activeCoroutines[id] = coroutine;
        }
    }

    private void RemoveCoroutine(string id) {
        if (activeCoroutines.ContainsKey(id)) {
            activeCoroutines[id].Stop();
            activeCoroutines.Remove(id);
        }
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
            TimedActionCoroutine(id, onStart, onUpdate, onComplete, duration),
            duration,
            delay
        );
    }

    private IEnumerator TimedActionCoroutine(
            string id,
            Action onStart, 
            Action<float> onUpdate, 
            Action onComplete, 
            float duration) {

        onStart?.Invoke();
        float time = 0f;
        while (time < duration) {
            onUpdate?.Invoke(time/duration);
            time += Time.deltaTime;
            yield return null;
        }
        onComplete?.Invoke();
        activeCoroutines.Remove(id);
        // onCleanup?.Invoke();
    }
}

public enum TransformType {
    Position,
    Rotation,
    Scale
}

public class TransformCoroutineManager : CoroutineManager {
    private TargetTransform targetTransform;
    private bool useLocal;

    public Vector3 TargetPosition { get { return targetTransform.targetPosition; } }
    public Quaternion TargetRotation { get { return targetTransform.targetRotation; } }
    public Vector3 TargetScale { get { return targetTransform.targetScale; } }

    private Dictionary<TransformType, float> cachedDurations = new Dictionary<TransformType, float>();
    private Action onTransformStart;
    private Action onTransformComplete;

    private bool shouldBreakCoroutine = false;
    
    // public TransformCoroutineManager(MonoBehaviour owner, bool useLocal = false, Action onTransformStart = null, Action onTransformComplete = null) : base(owner) {
    //     this.useLocal = useLocal;
    //     if(this.useLocal) {
    //         this.targetTransform = new TargetTransform(owner.transform.localPosition, owner.transform.localRotation, owner.transform.localScale);
    //     }
    //     else {
    //         this.targetTransform = new TargetTransform(owner.transform.position, owner.transform.rotation, owner.transform.localScale);
    //     }
    //     this.onTransformStart = onTransformStart;
    //     this.onTransformComplete = onTransformComplete;
    // }

    public TransformCoroutineManager(MonoBehaviour owner, Action onTransformStart = null, Action onTransformComplete = null, bool useLocal = false) : base(owner) {
        Vector3 position = useLocal ? owner.transform.localPosition : owner.transform.position;
        Quaternion rotation = useLocal ? owner.transform.localRotation : owner.transform.rotation;
        this.targetTransform = new TargetTransform(position, rotation, owner.transform.localScale);
        this.onTransformStart = onTransformStart;
        this.onTransformComplete = onTransformComplete;
    }

    /// <summary>
    /// Efficiently update position of owner MonoBehavior transform 
    /// </summary>
    public void MoveTo(Vector3 position, float duration = 1f) {
        if (Vector3.Equals(targetTransform.targetPosition, position))
            return;
        targetTransform.targetPosition = position;
        // check if the dict contains the timed position, also redundantly check if the coroutine is null (probably don't need this)
        if (activeCoroutines.ContainsKey("TimedPosition") && activeCoroutines["TimedPosition"] != null) {
            // Debug.Log($"Coroutine exists, extending duration by {duration}");
            // just extend the duration out, and update the target position
            cachedDurations[TransformType.Position] += duration;
            return;
        }
        cachedDurations[TransformType.Position] = duration;
        // if the coroutine is null, or the coroutine is not in the dict, then start a new coroutine
        activeCoroutines["TimedPosition"] = new ManagedCoroutine(
            owner, 
            TimedCachedDurationCoroutine(
                "TimedPosition", 
                (float t) => { 
                    if(useLocal) {
                        owner.transform.localPosition = Vector3.Lerp(owner.transform.localPosition, targetTransform.targetPosition, t);
                    }
                    else {
                        owner.transform.position = Vector3.Lerp(owner.transform.position, targetTransform.targetPosition, t);
                    }
                }, 
                duration,
                TransformType.Position
            ),
            duration
        );
    }

    /// <summary>
    /// Efficiently update rotation of owner MonoBehavior transform 
    /// </summary>
    public void RotateTo(Quaternion rotation, float duration = 1f) {
        if (Quaternion.Equals(targetTransform.targetRotation, rotation))
            return;
        targetTransform.targetRotation = rotation;
        if (activeCoroutines.ContainsKey("TimedRotation") && activeCoroutines["TimedRotation"] != null) {
            // Debug.Log($"Coroutine exists, extending duration by {duration}");
            cachedDurations[TransformType.Rotation] += duration;
            return;
        }
        cachedDurations[TransformType.Rotation] = duration;
        activeCoroutines["TimedRotation"] = new ManagedCoroutine(
            owner, 
            TimedCachedDurationCoroutine(
                "TimedRotation", 
                (float t) => { 
                    if(useLocal) {
                        owner.transform.localRotation = Quaternion.Lerp(owner.transform.localRotation, targetTransform.targetRotation, t);
                    }
                    else {
                        owner.transform.rotation = Quaternion.Lerp(owner.transform.rotation, targetTransform.targetRotation, t);
                    }
                }, 
                duration,
                TransformType.Rotation
            ),
            duration
        );
    }
    /// <summary>
    /// Efficiently update scale of owner MonoBehavior transform 
    /// </summary>
    public void ScaleTo(Vector3 scale, float duration = 1f) {
        if (Vector3.Equals(targetTransform.targetScale, scale))
            return;
        targetTransform.targetScale = scale;
        if (activeCoroutines.ContainsKey("TimedScale") && activeCoroutines["TimedScale"] != null) {
            // Debug.Log($"Coroutine exists, extending duration by {duration}");
            cachedDurations[TransformType.Scale] += duration;
            return;
        }
        cachedDurations[TransformType.Scale] = duration;
        activeCoroutines["TimedScale"] = new ManagedCoroutine(
            owner, 
            TimedCachedDurationCoroutine(
                "TimedScale", 
                (float t) => { owner.transform.localScale = Vector3.Lerp(owner.transform.localScale, targetTransform.targetScale, t); }, 
                duration,
                TransformType.Scale
            ),
            duration
        );
    }

    public void ResetDurations(float duration = 1f) {
        cachedDurations[TransformType.Position] = duration;
        cachedDurations[TransformType.Rotation] = duration;
        cachedDurations[TransformType.Scale] = duration;
    }

    public void TransformTo(TransformSnapshot snapshot, float duration = 1f) {
        MoveTo(snapshot.Position, duration);
        RotateTo(snapshot.Rotation, duration);
        ScaleTo(snapshot.Scale, duration);
    }

    public void SetTargetTransform(TransformSnapshot snapshot) {
        targetTransform.targetPosition = snapshot.Position;
        targetTransform.targetRotation = snapshot.Rotation;
        targetTransform.targetScale = snapshot.Scale;
    }

    public void Freeze(TransformType? transformType = null) {
        if (transformType == null) {
            if (activeCoroutines.ContainsKey("TimedPosition") && activeCoroutines["TimedPosition"] != null) {
                activeCoroutines["TimedPosition"].Stop();
                activeCoroutines["TimedPosition"] = null;
            }
            if (activeCoroutines.ContainsKey("TimedRotation") && activeCoroutines["TimedRotation"] != null) {
                activeCoroutines["TimedRotation"].Stop();
                activeCoroutines["TimedRotation"] = null;
            }
            if (activeCoroutines.ContainsKey("TimedScale") && activeCoroutines["TimedScale"] != null) {
                activeCoroutines["TimedScale"].Stop();
                activeCoroutines["TimedScale"] = null;
            }
            if(!useLocal) {
                targetTransform.targetPosition = owner.transform.position;
                targetTransform.targetRotation = owner.transform.rotation;
            }
            else {
                targetTransform.targetPosition = owner.transform.localPosition;
                targetTransform.targetRotation = owner.transform.localRotation;
            }
            targetTransform.targetScale = owner.transform.localScale;
            onTransformComplete?.Invoke();
            return;
        }
        switch (transformType) {
            case TransformType.Position:
                activeCoroutines["TimedPosition"].Stop();
                activeCoroutines["TimedPosition"] = null;
                if(!useLocal) {
                    targetTransform.targetPosition = owner.transform.position;
                }
                else {
                    targetTransform.targetPosition = owner.transform.localPosition;
                }
                break;
            case TransformType.Rotation:
                activeCoroutines["TimedRotation"].Stop();
                activeCoroutines["TimedRotation"] = null;
                if(!useLocal) {
                    targetTransform.targetRotation = owner.transform.rotation;
                }
                else {
                    targetTransform.targetRotation = owner.transform.localRotation;
                }
                break;
            case TransformType.Scale:
                activeCoroutines["TimedScale"].Stop();
                activeCoroutines["TimedScale"] = null;
                targetTransform.targetScale = owner.transform.localScale;
                break;
        }
        if (!AnyActiveCoroutines())
            onTransformComplete?.Invoke();
    }




    private bool AnyActiveCoroutines() {
        if (activeCoroutines.ContainsKey("TimedPosition") && activeCoroutines["TimedPosition"] != null) {
            return true;
        }
        if (activeCoroutines.ContainsKey("TimedRotation") && activeCoroutines["TimedRotation"] != null) {
            return true;
        }
        if (activeCoroutines.ContainsKey("TimedScale") && activeCoroutines["TimedScale"] != null) {
            return true;
        }
        return false;
    }

    // public bool 

    private IEnumerator TimedCachedDurationCoroutine(
            string id,
            Action<float> onUpdate, 
            float duration,
            TransformType transformType) {
        onTransformStart?.Invoke();
        float time = 0f;
        while (time < cachedDurations[transformType]) {
            if (shouldBreakCoroutine) {
                shouldBreakCoroutine = false;
                break;
            }
            onUpdate?.Invoke(time/cachedDurations[transformType]);
            // Debug.Log("TimedCachedDurationCoroutine:" + owner.name + " Frac:" + time/cachedDurations[transformType] + " Time:" + time + "/" + cachedDurations[transformType] + " Type:" + transformType);
            time += Time.deltaTime;
            yield return null;
        }
        activeCoroutines.Remove(id);
        if (!AnyActiveCoroutines())
            onTransformComplete?.Invoke();
    }
}


















// public void TimedTransform(string id, TargetTransform targetTransform, float duration, Action onStart = null, Action onComplete = null, float delay = 0f) {
//     if (id == null)
//         id = "TimedTransform";

//     if (activeCoroutines.ContainsKey(id))
//         activeCoroutines[id].Stop();

//     activeCoroutines[id] = new ManagedCoroutine(
//         owner, 
//         TimedTransformCoroutine(onStart, targetTransform, onComplete, duration, () => { activeCoroutines.Remove(id); }),
//         duration,
//         delay
//     );
// }

// private IEnumerator TimedTransformCoroutine(
//         Action onStart, 
//         TargetTransform targetTransform, 
//         Action onComplete, 
//         float duration,
//         Action onCleanup) {

//     onStart?.Invoke();
//     float time = 0f;
//     Vector3 startPosition = this.targetTransform.position;
//     Quaternion startRotation = this.targetTransform.rotation;
//     Vector3 startScale = this.targetTransform.localScale;
//     while (time < duration) {
//         this.targetTransform.position = Vector3.Lerp(startPosition, targetTransform.targetPosition, time/duration);
//         this.targetTransform.rotation = Quaternion.Lerp(startRotation, targetTransform.targetRotation, time/duration);
//         this.targetTransform.localScale = Vector3.Lerp(startScale, targetTransform.targetScale, time/duration);
//         time += Time.deltaTime;
//         yield return null;
//     }
//     this.targetTransform.position = targetTransform.targetPosition;
//     this.targetTransform.rotation = targetTransform.targetRotation;
//     this.targetTransform.localScale = targetTransform.targetScale;
//     onComplete?.Invoke();
//     onCleanup?.Invoke();
// }


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