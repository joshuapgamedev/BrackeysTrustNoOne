using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrongHoleTrigger : MonoBehaviour
{
    public static event System.Action OnWrongHoleAttempt;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("ObjectCollider"))
            return;

        ObjectController obj = other.GetComponentInParent<ObjectController>();

        if (obj == null)
            return;

        if (!obj.CompareTag("Holdable"))
            return;

        if (!PlayerController.Instance.IsHolding(obj))
            return;

        Debug.Log($"Wrong hole attempt: {obj.objectId}");

        OnWrongHoleAttempt?.Invoke();
    }
}
