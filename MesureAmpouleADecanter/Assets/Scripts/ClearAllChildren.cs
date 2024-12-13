using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ClearAllChildren
{
    public static void _ClearAllChildren(GameObject Parent)
    {
        while (Parent.transform.childCount > 0)
            GameObject.DestroyImmediate(Parent.transform.GetChild(0).gameObject);
    }
}
