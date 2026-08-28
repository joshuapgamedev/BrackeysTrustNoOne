using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class NarratorController : MonoBehaviour
{
    [SerializeField] private NarratorSO welcomeScript;
    [SerializeField] private GameObject narratorBox;
    [SerializeField] private TypewriterEffect narratorText;

    public static NarratorController Instance { get; private set; }

    private bool waitForNextLine = false;
    private bool gameStart = false;

    private bool playerActionCompleted = false;
    private bool actionWasEarly = false;
    private bool playerLeftTrigger = false;

    private bool playerCompletedPuzzle = false;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        //StartScript(welcomeScript);
        PlayerController.CanMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Keyboard.current.enterKey.wasPressedThisFrame
                || Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (waitForNextLine)
            {
                waitForNextLine = false;
            }

            if (narratorBox.activeSelf && narratorText.IsTyping)
            {
                narratorText.SkipToEnd();
            }

        }

    }

    private void OnEnable()
    {
        PlayerController.OnPlayerJumped += HandlePlayerJumped;
        PlayerController.OnPlayerInteracted += HandlePlayerInteract;
        PuzzleManager.OnPlayerCompletedPuzzle += HandlePlayerCompletedPuzzle;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerJumped -= HandlePlayerJumped;
        PlayerController.OnPlayerInteracted -= HandlePlayerInteract;
        PuzzleManager.OnPlayerCompletedPuzzle -= HandlePlayerCompletedPuzzle;
    }

    private void HandlePlayerJumped()
    {
        playerActionCompleted = true;

        if (narratorText.IsTyping)
        {
            actionWasEarly = true;
        }
    }
    public void PlayerLeftTrigger()
    {
        playerLeftTrigger = true;
    }

    private void HandlePlayerInteract()
    {
        playerActionCompleted = true;

        if (narratorText.IsTyping)
            actionWasEarly = true;
    }

    private void HandlePlayerCompletedPuzzle()
    {
        playerCompletedPuzzle = true;
    }
    public void UpdateNarratorUIText(string text)
    {
        narratorBox.SetActive(true);
        narratorText.ShowText(text);
    }

    public void StartScript(NarratorSO scriptData)
    {
        StartCoroutine(PlayScript(scriptData));
    }

    public IEnumerator PlayScript(NarratorSO scriptData)
    {
        int seq = 1;
        string currentSequenceID = scriptData.sequences[0].sequenceID;

        while (!string.IsNullOrEmpty(currentSequenceID))
        {
            NarratorSequence current = scriptData.sequences.Find(s => s.sequenceID == currentSequenceID);

            if (current == null)
            {
                Debug.Log($"Sequence not found: {currentSequenceID}");
                currentSequenceID = null;
                break;
            }

            Debug.Log($"Playing sequence: {current.sequenceID}");

            foreach (NarratorLine line in current.dialogue)
            {
                UpdateNarratorUIText(line.text);
                yield return new WaitForSeconds(line.delayAfter);

                ManageTask(current.taskUpdate);
            }

            NarratorResponse response = null;

            switch (current.waitType)
            {
                case NarratorWaitType.None:
                    //StartCoroutine(WaitForTypewriter(.5f));
                    yield return new WaitUntil(() => narratorText.IsTyping == false);
                    yield return new WaitForSeconds(1f);
                    currentSequenceID = null;
                    break;

                case NarratorWaitType.NextLineTrigger:
                    //StartCoroutine(WaitForTypewriter(.3f));
                    yield return new WaitUntil(() => narratorText.IsTyping == false);
                    yield return new WaitForSeconds(.3f);
                    waitForNextLine = true;

                    while (waitForNextLine)
                        yield return null;

                    //currentSequenceID = GetDefaultNextSequence(current);
                    currentSequenceID = (++seq).ToString();
                    break;

                case NarratorWaitType.Timer:
                    yield return new WaitForSeconds(current.timeout);

                    response = GetResponse(current, NarratorEvent.Timeout);
                    if(response == null)
                        currentSequenceID = (++seq).ToString();
                    break;

                case NarratorWaitType.PlayerInteract:
                case NarratorWaitType.PlayerJump:

                    StartCoroutine(WaitForTypewriter(.5f));
                    yield return StartCoroutine(WaitForPlayerAction(current));

                    /*
                    playerActionCompleted = false;
                    actionWasEarly = false;
                    playerLeftTrigger = false;

                    float timer = 0f;

                    while (!playerActionCompleted && timer < current.timeout)
                    {
                        timer += Time.deltaTime;
                        yield return null;
                    }
                    */

                    if (playerActionCompleted)
                    {
                        response = GetResponse(current, actionWasEarly ? NarratorEvent.InteractedEarly : NarratorEvent.CompletedNormally);
                    }
                    else if (playerLeftTrigger)
                    {
                        response = GetResponse(current, NarratorEvent.PlayerLeft);
                    }
                    else
                    {
                        response = GetResponse(current, NarratorEvent.Timeout);
                    }

                    break;

                case NarratorWaitType.PlayerCompletePressurePlatePuzzle:
                case NarratorWaitType.PlayerCompleteLeverPuzzle:

                    yield return new WaitUntil(() => narratorText.IsTyping == false);
                    yield return new WaitForSeconds(1f);
                    narratorBox.SetActive(false);

                    yield return StartCoroutine(WaitForPlayerToCompletePuzzle(current));

                    if (playerCompletedPuzzle)
                    {
                        response = GetResponse(current, actionWasEarly ? NarratorEvent.InteractedEarly : NarratorEvent.CompletedNormally);
                    }
                    break;

            }

            if (response != null)
            {
                ManageTask(response.taskUpdate);
                yield return StartCoroutine( PlayResponse(response) );

                Debug.Log($"nextSequenceID: {response.nextSequenceID}");
                currentSequenceID = response.nextSequenceID;
            }
            

            //seq++;
        }

        narratorBox.SetActive(false);

        // only trigger once after welcome text
        if (!gameStart)
        {
            gameStart = true;
            PlayerController.CanMove = true;
        }
    }

    private IEnumerator PlayResponse(NarratorResponse response)
    {
        foreach (NarratorLine line in response.dialogue)
        {
            UpdateNarratorUIText(line.text);
            //StartCoroutine(WaitForTypewriter(.3f));

            yield return new WaitUntil(() => narratorText.IsTyping == false);
            yield return new WaitForSeconds(.3f);

            if(line.delayAfter > 0)
            {
                yield return new WaitForSeconds(line.delayAfter);
            }
        }
    }

    private NarratorResponse GetResponse(NarratorSequence sequence, NarratorEvent eventType)
    {
        return sequence.responses.Find(response => response.eventType == eventType);
    }

    private IEnumerator WaitForPlayerAction(NarratorSequence current)
    {
        playerActionCompleted = false;
        actionWasEarly = false;
        playerLeftTrigger = false;

        float timer = 0f;

        while (!playerActionCompleted && !playerLeftTrigger && timer < current.timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator WaitForPlayerToCompletePuzzle(NarratorSequence current)
    {
        playerCompletedPuzzle = false;
        while(!playerCompletedPuzzle)
        {
            yield return null;
        }
        
    }

    private void ManageTask(TaskUpdate taskEvent)
    {
        //Debug.Log("ManagerTask called");
        if (taskEvent.action != TaskAction.None)
        {
            switch (taskEvent.action)
            {
                case TaskAction.Complete:
                    TaskListManager.Instance.CompleteTask(taskEvent.taskID);
                    break;
                case TaskAction.Fail:
                    break;
                case TaskAction.Remove:
                    TaskListManager.Instance.ClearTask();
                    break;
                case TaskAction.Add:
                    TaskListManager.Instance.DisplayTask(taskEvent.taskID);
                    break;
            }
            
        }
    }

    private IEnumerator WaitForTypewriter(float time)
    {
        yield return new WaitUntil(() => narratorText.IsTyping == false);
        yield return new WaitForSeconds(time);
        
    }
}
