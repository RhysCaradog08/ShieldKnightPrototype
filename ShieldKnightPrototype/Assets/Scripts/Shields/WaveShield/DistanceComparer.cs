using System.Collections;
using System;
using UnityEngine;

public class DistanceComparer : IComparer
{
    private Transform compareTransform;

    public DistanceComparer(Transform compTransform)
    {
        compareTransform = compTransform;
    }

    public int Compare (object a, object b)
    {
        Transform aTransform = a as Transform;
        Transform bTransform = b as Transform;

        Vector3 offset = aTransform.position - compareTransform.position;
        float aDistance = offset.sqrMagnitude;

        offset = bTransform.position - compareTransform.position;
        float bDistance = offset.sqrMagnitude;

        return aDistance.CompareTo(bDistance);
    }


}
