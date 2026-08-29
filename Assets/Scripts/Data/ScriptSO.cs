using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Narrator/Script")]
public class ScriptSO : ScriptableObject
{
    public string narratorID;

    public List<ScriptBlock> blocks;
}

[System.Serializable]
public class ScriptBlock
{
    public string blockID;

    // block triggered
    public ScriptRunCondition runCondition;

    public List<ScriptLine> dialogue;

    // gameplay event
    public ScriptWaitType waitType = ScriptWaitType.None;

    public float timeout = 0f;

    public List<ScriptResponse> responses;

    // continue
    public string nextBlockID;

    public TaskUpdate taskUpdate;

    public bool randomResponse = false;
}

public enum ScriptRunCondition
{
    Default,
    Partial,
    Failed,
    Completed,
    Secret,
    None
}

[System.Serializable]
public class ScriptLine
{
    [TextArea]
    public string text;

    public AudioClip audio;

    // line progress
    public ScriptNextLineType nextLineType = ScriptNextLineType.None;

    public float delayAfter = 0f;

    public bool canBeInterrupted = true;
}

[System.Serializable]
public class ScriptResponse
{
    public ScriptEvent eventType;

    public List<ScriptLine> dialogue;

    public string nextBlockID;

    public TaskUpdate taskUpdate;
}

public enum ScriptEvent
{
    CompletedNormally,
    CompletedEarly,
    Timeout,
    PlayerLeft,

    CorrectButton,
    WrongButton
}

public enum ScriptNextLineType
{
    None,
    Timer,
    NextLineTrigger,
}
public enum ScriptWaitType
{
    None,

    PlayerJump,
    PlayerInteract,

    PlayerTriggerThreeLevers,
    PlayerCompleteLeverPuzzle,

    PlayerActivateOnePressurePlate,
    PlayerActivateAllPressurePlates,

    PlayerTryWrongHole,

    PlayerPressButton,

}