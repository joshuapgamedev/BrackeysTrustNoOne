using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarratorZone : MonoBehaviour
{
    [SerializeField] public NarratorSO narrator;
    [SerializeField] public ScriptSO script;
    [SerializeField] private bool canTriggerAgain = false;

    private bool hasTriggered = false;

    public static event System.Action<PuzzleRoomType> OnPlayerEnterPuzzle;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!hasTriggered)
        {
            hasTriggered = true;

            //NarratorController.Instance.StartScript(script);
            ScriptController.Instance.StartScript(script);
        }

        if(canTriggerAgain)
            hasTriggered = false;

        OnPlayerEnterPuzzle?.Invoke(GetRoomType(script.narratorID));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        NarratorController.Instance.PlayerLeftTrigger();
    }

    private PuzzleRoomType GetRoomType(string narratorID)
    {
        if (narratorID == "Lever")
            return PuzzleRoomType.Lever;
        else if (narratorID == "PressurePlate")
            return PuzzleRoomType.PressurePlate;
        else if (narratorID == "SquareHole")
            return PuzzleRoomType.SquareHole;
        else if (narratorID == "Button")
            return PuzzleRoomType.Button;
        else if (narratorID == "Final")
            return PuzzleRoomType.Final;
        else
            return PuzzleRoomType.None;
    }
}
