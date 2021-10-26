using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class MarkerCheck : MonoBehaviour
{
    public GameObject marker;

    public bool canAddMarker = true;

    public void AddMarker()
    {
        Vector3 markerPos = transform.position;

        marker = ObjectPoolManager.instance.CallObject("TargetMarker", transform, new Vector3(markerPos.x, markerPos.y + (transform.localScale.y + 1), markerPos.z - 0.65f), Quaternion.identity);

        canAddMarker = false;
    }

    public void RemoveMarker()
    {
        ObjectPoolManager.instance.RecallObject(marker);

        canAddMarker = true;
    }
}
