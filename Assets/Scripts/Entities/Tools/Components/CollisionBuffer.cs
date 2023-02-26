using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using System;

public class CollisionEvent
{
    public GameObject gameObject;
    public float expiresAt;

    public void Reset()
    {
        gameObject = null;
    }
}

public class CollisionBuffer : MonoBehaviour
{
    public string collisionTag = "grain";
    public float lifetime = 0.1f;
    public List<CollisionEvent> collisionBuffer;

    private ObjectPool<CollisionEvent> collisionWrapperPool;
    private HashSet<GameObject> unusedObjects;
    private List<int> indexesToRemove;

    private void Start()
    {
        collisionBuffer = new List<CollisionEvent>();
        unusedObjects = new HashSet<GameObject>();
        collisionWrapperPool = new ObjectPool<CollisionEvent>(() => new CollisionEvent(), wrapper => wrapper.Reset());
        indexesToRemove = new List<int>();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag(collisionTag))
        {
            if (unusedObjects.Contains(collision.gameObject) && UnityEngine.Random.value > 0.5f) {
                return;
            }
            unusedObjects.Add(collision.gameObject);

            CollisionEvent wrapper = collisionWrapperPool.Get(); // get from the pool
            wrapper.gameObject = collision.gameObject;
            wrapper.expiresAt = Time.time + lifetime;
            collisionBuffer.Add(wrapper);
        }
    }

    private void FixedUpdate()
    {
        var currentTime = Time.time;
        indexesToRemove.Clear();

        for(int i = 0; i < collisionBuffer.Count; i++) {
            CollisionEvent wrapper = collisionBuffer[i];
            if (wrapper.expiresAt < currentTime)
            {
                collisionWrapperPool.Release(wrapper); // return to the pool
                // collisionBuffer.RemoveAt(i);
                indexesToRemove.Add(i);
            }
        }

        for (int j = indexesToRemove.Count - 1; j >= 0; j--) {
            collisionBuffer.RemoveAt(indexesToRemove[j]);
        }
    }

    // private void FixedUpdate()
    // {
    //     var currentTime = Time.time;
    //     var expired = collisionBuffer.FindAll(wrapper => wrapper.expiresAt < currentTime);
        
    //     foreach (CollisionEvent wrapper in expired) {
    //         collisionWrapperPool.Release(wrapper); // return to the pool
    //         collisionBuffer.Remove(wrapper);
    //     }
    // }



    public void ClearBuffer() {
        collisionBuffer.Clear();
    }

    public void RunActionOnColliders(Action<CollisionEvent> action) {
        foreach (CollisionEvent e in collisionBuffer) {
            if (e.gameObject == null) continue;
            unusedObjects.Remove(e.gameObject);
            action(e);
        }
    }
}