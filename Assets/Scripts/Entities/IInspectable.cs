using System.Collections.Generic;
using UnityEngine;
using System;

public struct InspectableProperty
{
    public string Name;
    public string Description;
    public Action OnSelect;
}


public struct InspectableProperties
{
    public string Name;
    public Transform Transform;
    public List<InspectableProperty> Properties;
}


public interface IInspectable {
    InspectableProperties Inspect();
    // have some sort of inspectable scope, so that we don't try to inspect
    // all the sub items in an inspectable collection like a grainCloud
}
