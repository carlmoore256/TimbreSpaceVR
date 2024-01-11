using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public interface INode
{
    Guid Id { get; set; }
    // public List<INode> children;
}

public static class NodeFactory {
    public static T CreateNode<T>(GameObject prefab, Transform parent) where T : INode {
        var go = GameObject.Instantiate(prefab, parent);
        var node = go.GetComponent<T>();
        return node;
    }
}