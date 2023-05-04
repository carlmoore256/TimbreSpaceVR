using UnityEngine;

public enum PlacementState
{
    Placed,
    Unplaced
}

public interface IMovable
{
    PlacementState PlacementState { get; set; } // A property that indicates whether the object is placed or not
    TransformSnapshot CurrentTransform { get; } // A snapshot of the object's current transform
    void MoveTo(TransformSnapshot transformTarget, float duration); // A method that updates the object's position while it's being moved
    void OnMoveStart(); // A method that gets called when an object starts to be moved
    void OnMoveEnd(); // A method that gets called when the object is no longer being moved
}