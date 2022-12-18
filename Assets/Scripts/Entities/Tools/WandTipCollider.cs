using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handle adding and removing current colliders from a publicly available buffer
public class WandTipCollider : MonoBehaviour
{
    private List<string> colliderTags = new List<string> { "grain" };
    public List<Collider> ColliderBuffer { get; protected set; }

    public void SetColliderTags(List<string> tags) {
        colliderTags = tags;
    }
    public void AddColliderTag(string tag) {
        colliderTags.Add(tag);
    }
    public void RemoveColliderTag(string tag) {
        colliderTags.Remove(tag);
    }

    void OnEnable()
    {
        ColliderBuffer = new List<Collider>();
    }

    private void OnCollisionEnter(Collision collision) {
        if (colliderTags.Contains(collision.gameObject.tag)) {
            ColliderBuffer.Add(collision.collider);
        }
    }

    private void OnCollisionExit(Collision collision) {
        if (colliderTags.Contains(collision.gameObject.tag)) {
            if (ColliderBuffer.Contains(collision.collider)) {
                ColliderBuffer.Remove(collision.collider);
            } else {
                Debug.LogError("Collider buffer does not contain collider!");
            }
        }
    }
}
