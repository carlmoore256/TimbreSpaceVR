using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Handle adding and removing current colliders from a publicly available buffer
public class WandTipCollider : MonoBehaviour
{

    class CollisionEvent {
        public Collider collider;
        public float time;
        public CollisionEvent(Collider collider) {
            this.collider = collider;
            this.time = Time.time;
        }
    }
    private int maxQueueSize = 1024;
    private int numFramesThreshold = 2;
    private string colliderTag = "grain";
    private Queue<CollisionEvent> collisionEventQueue = new Queue<CollisionEvent>();

    void OnEnable()
    {
        collisionEventQueue = new Queue<CollisionEvent>();
    }

    public int CollisionQueueStats() {
        return collisionEventQueue.Count;
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.CompareTag(colliderTag)) {
            if (collisionEventQueue.Count > maxQueueSize) {
                collisionEventQueue.Dequeue();
            }
            collisionEventQueue.Enqueue(new CollisionEvent(collision.collider));
        }
    }

    private void OnCollisionStay(Collision collision) {
        if (collision.collider.CompareTag(colliderTag)) {
            if (collisionEventQueue.Count > maxQueueSize) {
                collisionEventQueue.Dequeue();
            }
            collisionEventQueue.Enqueue(new CollisionEvent(collision.collider));
        }
    }

    public void RunActionOnColliders(Action<GameObject> action) {
        float thresholdTime = Time.time - (Time.deltaTime * numFramesThreshold);
        foreach (CollisionEvent collisionEvent in collisionEventQueue) {
            if (collisionEvent.time < thresholdTime) continue;
            action.Invoke(collisionEvent.collider.gameObject);
        }
    }

    public void PlayCollidedGrains(float gain) {
        float thresholdTime = Time.time - (Time.deltaTime * numFramesThreshold);
        foreach (CollisionEvent collisionEvent in collisionEventQueue) {
            if (collisionEvent.time < thresholdTime) continue;
            collisionEvent.collider.gameObject.GetComponent<Grain>().PlayGrain(gain);
        }
    }
}





















// public void ClearColliderBuffer() {
//     ActiveColliderBuffer.Clear();
// }
// public void SetColliderTags(List<string> tags) {
//     colliderTags = tags;
// }
// public void AddColliderTag(string tag) {
//     colliderTags.Add(tag);
// }
// public void RemoveColliderTag(string tag) {
//     colliderTags.Remove(tag);
// }

// private void OnCollisionEnter(Collision collision) {
//     if (collision.gameObject.CompareTag(colliderTag)) {
//         // ActiveColliderBuffer.Add(collision.collider);
//         if (entranceQueue.Count > maxQueueSize)
//             entranceQueue.Dequeue();
//         entranceQueue.Enqueue(collision.collider);
//     }
// }

// private void OnCollisionExit(Collision collision) {
//     // if (colliderTags.Contains(collision.gameObject.tag)) {
//     if (collision.gameObject.CompareTag(colliderTag)) {
//         if (ActiveColliderBuffer.Contains(collision.collider)) {
//             ActiveColliderBuffer.Remove(collision.collider);
//         } else {
//             Debug.LogError("Collider buffer does not contain collider!");
//         }
//     }
// }