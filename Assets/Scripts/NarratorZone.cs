using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarratorZone : MonoBehaviour
{
    [SerializeField] public NarratorSO script;
    [SerializeField] private bool canTriggerAgain = false;

    private bool hasTriggered = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!hasTriggered)
        {
            hasTriggered = true;

            NarratorController.Instance.StartScript(script);
        }

        if(canTriggerAgain)
            hasTriggered = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        NarratorController.Instance.PlayerLeftTrigger();
    }

}
