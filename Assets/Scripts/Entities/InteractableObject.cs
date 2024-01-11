using UnityEngine;

public abstract class InteractableObject : MonoBehaviour, IInspectable, IMovable
{

    public PlacementState PlacementState { get; set; }
    public TransformSnapshot CurrentTransform { get {
        return new TransformSnapshot(gameObject.transform);
    } }
    
    protected TransformCoroutineManager _transformCoroutineManager;

    public virtual void OnEnable()
    {
        _transformCoroutineManager = new TransformCoroutineManager(this, OnMoveStart, OnMoveEnd);
    }

    public void MoveTo(TransformSnapshot transformTarget, float duration = 0.5f)
    {
        _transformCoroutineManager.TransformTo(transformTarget, duration);        
    }
    
    public virtual void OnMoveStart()
    {
        PlacementState = PlacementState.Unplaced;
    }

    public virtual void OnMoveEnd()
    {
        PlacementState = PlacementState.Placed;
    }

    public abstract InspectableProperties Inspect();
}

// public abstract class PlayableObject : InteractiveObject
// {

// }