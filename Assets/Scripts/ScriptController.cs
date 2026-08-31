using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ScriptController : MonoBehaviour
{
    public static ScriptController Instance { get; private set; }

    [Header("UI")]
    [SerializeField] public ScriptSO openingScript;
    [SerializeField] private GameObject narratorBox;
    [SerializeField] private TypewriterEffect narratorText;

    [SerializeField] private AudioSource narratorAudioSource;

    private ScriptSO currentScript;
    private Coroutine currentRoutine;

    private ScriptWaitType currentWaitType = ScriptWaitType.None;

    private bool waitForNextLine;

    // Runtime event state
    private bool playerActionCompleted;
    private bool actionWasEarly;
    private bool playerLeftTrigger;
    private bool eventTimedOut = false;
    private bool isPlayingBlockDialogue;
    private bool interruptDialogue = false;

    private bool squareHolePartialTriggered = false;

    private ButtonPressResult? currentButtonResult = null;

    public bool gameStart = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame ||
            Keyboard.current.enterKey.wasPressedThisFrame)
        {
            // If text is still typing, first click just finishes it
            if (narratorBox.activeSelf && narratorText.IsTyping)
            {
                narratorText.SkipToEnd();
                return;
            }

            // Otherwise allow next line
            if (waitForNextLine)
            {
                waitForNextLine = false;
            }
        }
    }

    private void OnEnable()
    {
        PuzzleManager.OnLeverCountChanged += HandleLeverCountChanged;
        //PuzzleManager.OnPlayerCompletedPuzzle += HandlePuzzleCompleted;
        PuzzleManager.OnPressurePlateCountChanged += HandlePressurePlateCountChanged;
        WrongHoleTrigger.OnWrongHoleAttempt += HandleWrongHoleAttempt;

        PuzzleManager.OnPuzzleCompleted += HandlePuzzleCompleted;
        SquareHoleController.OnShapeInserted += HandleShapeInserted;
        PuzzleManager.OnButtonPressResult += HandleButtonPressResult;
        PuzzleManager.OnButtonPuzzleExpired += HandleButtonPuzzleExpired;
    }

    private void OnDisable()
    {
        PuzzleManager.OnLeverCountChanged -= HandleLeverCountChanged;
        //PuzzleManager.OnPlayerCompletedPuzzle -= HandlePuzzleCompleted;
        PuzzleManager.OnPressurePlateCountChanged -= HandlePressurePlateCountChanged;
        WrongHoleTrigger.OnWrongHoleAttempt -= HandleWrongHoleAttempt;

        PuzzleManager.OnPuzzleCompleted -= HandlePuzzleCompleted;
        SquareHoleController.OnShapeInserted -= HandleShapeInserted;
        PuzzleManager.OnButtonPressResult -= HandleButtonPressResult;
        PuzzleManager.OnButtonPuzzleExpired -= HandleButtonPuzzleExpired;
    }


    public void StartScript(ScriptSO script)
    {
        currentScript = script;

        TriggerCondition(ScriptRunCondition.Default);
    }

    public void TriggerCondition(ScriptRunCondition condition)
    {
        if (currentScript == null)
            return;

        /*
        ScriptBlock block = currentScript.blocks.Find(b => b.runCondition == condition);

        if (block == null)
        {
            Debug.LogWarning($"No ScriptBlock with condition {condition} " + $"in {currentScript.name}");

            return;
        }*/

        var matchingBlocks = currentScript.blocks.FindAll(b => b.runCondition == condition);

        if (matchingBlocks.Count == 0)
        {
            Debug.LogWarning(
                $"No ScriptBlock with condition {condition} " +
                $"in {currentScript.name}"
            );

            return;
        }

        ScriptBlock block;

        bool canRandomize = matchingBlocks.Count > 1 && matchingBlocks.Exists(b => b.allowRandomSelection);

        if (canRandomize)
        {
            var randomPool = matchingBlocks.FindAll(b => b.allowRandomSelection);

            block = randomPool[Random.Range(0, randomPool.Count)];
        }
        else
        {
            block = matchingBlocks[0];
        }


        Debug.Log($"block: {block.blockID}");
        PlayBlock(block);
    }

    private void PlayBlock(ScriptBlock block)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        currentWaitType = ScriptWaitType.None;
        waitForNextLine = false;
        interruptDialogue = false;

        if (narratorText.IsTyping)
        {
            narratorText.SkipToEnd();
        }

        if (narratorAudioSource.isPlaying)
        {
            narratorAudioSource.Stop();
        }

        currentRoutine = StartCoroutine(PlayBlockChain(block));
    }

    private IEnumerator PlayBlockChain(ScriptBlock startingBlock)
    {
        ScriptBlock current = startingBlock;

        while (current != null)
        {
            interruptDialogue = false;
            ScriptResponse response = null;

            ManageTask(current.taskUpdate);

            // listening
            ResetEventState();
            currentWaitType = current.waitType;

            // play all dialogue
            isPlayingBlockDialogue = true;

            foreach (ScriptLine line in current.dialogue)
            {
                if (interruptDialogue)
                    break;

                yield return StartCoroutine(PlayLine(line));

                if (interruptDialogue)
                    break;
            }

            isPlayingBlockDialogue = false;

            // wait for action event
            yield return StartCoroutine(WaitForBlockEvent(current));

            currentWaitType = ScriptWaitType.None;

            response = ResolveResponse(current);

            interruptDialogue = false;

            // response
            string nextBlockID = current.nextBlockID;

            if (response != null)
            {
                ManageTask(response.taskUpdate);

                foreach (ScriptLine line in response.dialogue)
                {
                    yield return StartCoroutine(PlayLine(line));
                }

                if (!string.IsNullOrEmpty(response.nextBlockID))
                {
                    nextBlockID = response.nextBlockID;
                }
            }

            // next block
            if (string.IsNullOrEmpty(nextBlockID))
            {
                current = null;
            }
            else
            {
                current = currentScript.blocks.Find(b => b.blockID == nextBlockID);

                if (current == null)
                {
                    Debug.LogWarning( $"Cannot find ScriptBlock: {nextBlockID}");
                }
            }
        }

        narratorBox.SetActive(false);
        currentRoutine = null;

        // only trigger once after welcome text
        if (!gameStart)
        {
            gameStart = true;
            PlayerController.CanMove = true;
        }
    }

    private IEnumerator PlayLine(ScriptLine line)
    {
        narratorBox.SetActive(true);

        if (line.audio != null)
        {
            narratorAudioSource.Stop();
            narratorAudioSource.clip = line.audio;
            narratorAudioSource.Play();
        }

        narratorText.ShowText(line.text);

        while (narratorText.IsTyping && !interruptDialogue)
        {
            yield return null;
        }

        if (interruptDialogue)
        {
            narratorAudioSource.Stop();
            yield break;
        }

        // typewriter
        //yield return new WaitUntil(() => !narratorText.IsTyping);

        switch (line.nextLineType)
        {
            case ScriptNextLineType.None:
                {
                    float timer = 0f;

                    while (timer < 1f && !interruptDialogue)
                    {
                        timer += Time.deltaTime;
                        yield return null;
                    }

                    if (interruptDialogue)
                        yield break;

                    narratorBox.SetActive(false);
                    break;
                }


            case ScriptNextLineType.Timer:
                {
                    float timer = 0f;

                    while (timer < line.delayAfter && !interruptDialogue)
                    {
                        timer += Time.deltaTime;
                        yield return null;
                    }

                    if (interruptDialogue)
                        yield break;

                    break;
                }


            case ScriptNextLineType.NextLineTrigger:
                {
                    waitForNextLine = true;

                    while (waitForNextLine && !interruptDialogue)
                    {
                        yield return null;
                    }

                    waitForNextLine = false;

                    if (interruptDialogue)
                        yield break;

                    break;
                }
        }
        /*
        switch (line.nextLineType)
        {
            case ScriptNextLineType.None:
                yield return new WaitUntil(() => narratorText.IsTyping == false);
                yield return new WaitForSeconds(1f);
                narratorBox.SetActive(false);
                break;


            case ScriptNextLineType.Timer:
                yield return new WaitUntil(() => narratorText.IsTyping == false);
                //yield return new WaitForSeconds(.3f);
                if (line.delayAfter > 0f)
                {
                    yield return new WaitForSeconds(line.delayAfter);
                }

                break;


            case ScriptNextLineType.NextLineTrigger:

                waitForNextLine = true;

                while (waitForNextLine)
                {
                    yield return null;
                }

                break;
        }
        */

    }

    private IEnumerator WaitForBlockEvent(
    ScriptBlock block)
    {
        if (block.waitType == ScriptWaitType.None)
            yield break;

        float timer = 0f;

        while (!playerActionCompleted && !playerLeftTrigger)
        {
            if (block.timeout > 0f)
            {
                timer += Time.deltaTime;

                if (timer >= block.timeout)
                {
                    eventTimedOut = true;
                    yield break;
                }
            }

            yield return null;
        }
    }

    private void ResetEventState()
    {
        playerActionCompleted = false;
        actionWasEarly = false;
        playerLeftTrigger = false;
        eventTimedOut = false;
        currentButtonResult = null;
    }

    private ScriptResponse ResolveResponse(ScriptBlock block)
    {
        if (block.responses == null || block.responses.Count == 0)
            return null;

        ScriptEvent result;

        if (currentButtonResult.HasValue)
        {
            result = currentButtonResult.Value == ButtonPressResult.Correct ? ScriptEvent.CorrectButton : ScriptEvent.WrongButton;
        }
        else if(playerLeftTrigger)
        {
            result = ScriptEvent.PlayerLeft;
        }
        else if (eventTimedOut)
        {
            result = ScriptEvent.Timeout;
        }
        else if (playerActionCompleted)
        {
            result = actionWasEarly ? ScriptEvent.CompletedEarly : ScriptEvent.CompletedNormally;
        }
        else
        {
            return null;
        }

        // random
        if (block.randomResponse)
        {
            var matchingResponses = block.responses.FindAll(r => r.eventType == result);

            if (matchingResponses.Count == 0)
                return null;

            return matchingResponses[Random.Range(0, matchingResponses.Count)];
        }

        return block.responses.Find(r => r.eventType == result);
    }

    private void ManageTask(TaskUpdate taskEvent)
    {
        if (taskEvent == null || taskEvent.action == TaskAction.None)
            return;
        

        switch (taskEvent.action)
        {
            case TaskAction.Add:
                TaskListManager.Instance.DisplayTask(taskEvent.taskID);
                break;

            case TaskAction.Complete:
                TaskListManager.Instance.CompleteTask(taskEvent.taskID);
                break;

            case TaskAction.Remove:
                TaskListManager.Instance.ClearTask();
                break;

            case TaskAction.Fail:
                break;
        }
    }

    private bool IsWaitConditionComplete(ScriptWaitType waitType)
    {
        switch (waitType)
        {
            case ScriptWaitType.PlayerJump:
                return playerActionCompleted;

            case ScriptWaitType.PlayerInteract:
                return playerActionCompleted;

            case ScriptWaitType.PlayerTriggerThreeLevers:
                return playerActionCompleted;

            case ScriptWaitType.PlayerCompleteLeverPuzzle:
                return playerActionCompleted;

            case ScriptWaitType.PlayerLeaveTrigger:
                return playerLeftTrigger;
        }

        return false;
    }


    private void HandleLeverCountChanged(int count)
    {
        Debug.Log($"currentWaitType ({currentWaitType})");
        if (currentWaitType != ScriptWaitType.PlayerTriggerThreeLevers)
            return;

        if (count >= 3)
        {
            CompleteCurrentWait();
        }
    }

    private void HandlePuzzleCompleted(
    PuzzleRoomType puzzleType)
    {
        switch (puzzleType)
        {
            case PuzzleRoomType.SquareHole:

                Debug.Log("Square Hole completed - interrupt narrator");

                TriggerCondition(ScriptRunCondition.Completed);

                break;


            case PuzzleRoomType.Lever:

                if (currentWaitType == ScriptWaitType.PlayerCompleteLeverPuzzle)
                {
                    CompleteCurrentWait();
                }

                break;

            case PuzzleRoomType.Final:

                Debug.Log("Green button pressed - FINAL");

                SceneManager.LoadScene("Game_Over");
                //TriggerCondition(ScriptRunCondition.Completed);
                //change the scene
                break;
        }
    }

    private void HandlePressurePlateCountChanged(int current, int total)
    {
        if (currentWaitType == ScriptWaitType.PlayerActivateOnePressurePlate && current >= 1)
        {
            CompleteCurrentWait();
        }

        else if (currentWaitType == ScriptWaitType.PlayerActivateAllPressurePlates && current >= total)
        {
            CompleteCurrentWait();
        }
    }

    private void CompleteCurrentWait()
    {
        playerActionCompleted = true;

        if (isPlayingBlockDialogue)
        {
            actionWasEarly = true;
            interruptDialogue = true;

            if (narratorText.IsTyping)
            {
                narratorText.SkipToEnd();
            }
        }
    }

    private void HandleWrongHoleAttempt()
    {
        if (currentWaitType != ScriptWaitType.PlayerTryWrongHole)
            return;

        CompleteCurrentWait();
    }

    private void HandleShapeInserted(ObjectController shape)
    {
        if (!squareHolePartialTriggered)
        {
            squareHolePartialTriggered = true;

            TriggerCondition(
                ScriptRunCondition.Partial
            );
        }
    }

    private void HandleButtonPressResult(
    ButtonPressResult result)
    {
        // COMPLETION IS GLOBAL
        if (result == ButtonPressResult.Completed)
        {
            TriggerCondition(ScriptRunCondition.Completed);
            return;
        }

        // Otherwise only care if current block
        // is waiting for a button press
        if (currentWaitType != ScriptWaitType.PlayerPressButton)
            return;

        currentButtonResult = result;

        CompleteCurrentWait();
    }

    private void HandleButtonPuzzleExpired()
    {
        TriggerCondition(ScriptRunCondition.Failed);
    }

    public void NotifyPlayerLeftTrigger()
    {
        if (currentWaitType != ScriptWaitType.PlayerLeaveTrigger)
            return;

        playerLeftTrigger = true;
        interruptDialogue = true;

        if (narratorAudioSource.isPlaying)
        {
            narratorAudioSource.Stop();
        }
    }

    public void PauseNarratorAudio()
    {
        if (narratorAudioSource != null && narratorAudioSource.isPlaying)
        {
            narratorAudioSource.Pause();
        }
    }

    public void ResumeNarratorAudio()
    {
        if (narratorAudioSource != null)
        {
            narratorAudioSource.UnPause();
        }
    }
}
