using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Narrator/Narrator Sequence")]
public class NarratorSO : ScriptableObject
{
    public string narratorID;

    public List<NarratorSequence> sequences;
}

[System.Serializable]
public class NarratorSequence
{
    public string sequenceID;

    public List<NarratorLine> dialogue;

    public NarratorWaitType waitType;

    public float timeout = 0f;

    public List<NarratorResponse> responses;

    public TaskUpdate taskUpdate;
}

public enum NarratorWaitType
{
    None,
    Timer,
    PlayerEnterTrigger,
    PlayerExitTrigger,
    NextLineTrigger,
    PlayerInteract,
    PlayerJump,
    PlayerMove,
    PlayerCrouch,
    PlayerCompleteLeverPuzzle,
    PlayerCompletePressurePlatePuzzle,
    CheckButtonPuzzle,
    PlayerCompleteButtonPuzzle
}

[System.Serializable]
public class NarratorResponse
{
    public NarratorEvent eventType;
    public string nextSequenceID;
    public List<NarratorLine> dialogue;
    public TaskUpdate taskUpdate;
}

[System.Serializable]
public class NarratorLine
{
    [TextArea]
    public string text;

    public AudioClip audio;

    public float delayAfter = 0f;

    public bool canBeInterrupted = true;
}

public enum NarratorEvent
{
    CompletedNormally,
    InteractedEarly,
    Timeout,
    PlayerLeft
}

public enum TaskAction
{
    None,
    Add,
    Complete,
    Fail,
    Remove
}

[System.Serializable]
public class TaskUpdate
{
    public TaskAction action;
    public string taskID;
}