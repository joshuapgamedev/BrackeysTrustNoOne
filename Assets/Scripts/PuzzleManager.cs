using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private List<ObjectController> levers;
    [SerializeField] private List<ObjectController> pressurePlates;
    [SerializeField] private List<ObjectController> buttons;
    [SerializeField] private int requiredShapes = 3;

    private PuzzleRoomType currentPuzzle;

    private int nextButtonIndex = 0;

    private int insertedShapeCount = 0;

    private bool triggeredThreeLevers = false;
    public static event System.Action<int> OnLeverCountChanged;

    public static event System.Action<int, int> OnPressurePlateCountChanged;

    private bool squareHoleCompleted = false;

    public static event System.Action<ButtonPressResult> OnButtonPressResult;
    public static event System.Action OnButtonPuzzleExpired;

    public static event System.Action OnPlayerCompletedPuzzle;
    public static event System.Action<PuzzleRoomType> OnPuzzleCompleted;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        NarratorZone.OnPlayerEnterPuzzle += ChoooseRoom;
        PlayerController.OnPlayerToggleLever += CheckPuzzle;
        ObjectController.OnActivatePressurePlate += CheckPuzzle;
        ObjectController.OnPressButton += CheckButtonPuzzleOrder;
        ObjectController.OnButtonExpired += HandleButtonExpired;
        SquareHoleController.OnShapeInserted += ShapeInserted;
    }

    private void OnDisable()
    {
        NarratorZone.OnPlayerEnterPuzzle -= ChoooseRoom;
        PlayerController.OnPlayerToggleLever -= CheckPuzzle;
        ObjectController.OnActivatePressurePlate -= CheckPuzzle;
        ObjectController.OnPressButton -= CheckButtonPuzzleOrder;
        ObjectController.OnButtonExpired -= HandleButtonExpired;
        SquareHoleController.OnShapeInserted -= ShapeInserted;
    }

    private void ChoooseRoom(PuzzleRoomType puzzleType)
    {
        currentPuzzle = puzzleType;

    }

    public void CheckPuzzle()
    {
        /*
        if (CompletedCountingTypePuzzle(currentPuzzle))
        {
            OnPlayerCompletedPuzzle?.Invoke();
            Debug.Log("Lever completed");
        }
        */
        switch (currentPuzzle)
        {
            case PuzzleRoomType.Lever:
                CheckLeverPuzzle();
                break;

            case PuzzleRoomType.PressurePlate:
                CheckPressurePlatePuzzle();
                break;
        }

        
    }

    private void CheckLeverPuzzle()
    {
        Debug.Log($"CheckPuzzle");

        if (currentPuzzle != PuzzleRoomType.Lever)
            return;

        int activatedCount = GetActivatedLeverCount();

        Debug.Log($"Activated levers: {activatedCount}/{levers.Count}");
        OnLeverCountChanged?.Invoke(activatedCount);

        if (activatedCount == levers.Count)
        {
            //OnPlayerCompletedPuzzle?.Invoke();
            OnPuzzleCompleted?.Invoke(PuzzleRoomType.Lever);
            Debug.Log("Lever completed");
        }
    }

    private void CheckPressurePlatePuzzle()
    {
        int activatedCount = GetActivatedPressurePlateCount();

        Debug.Log(
            $"Activated pressure plates: " +
            $"{activatedCount}/{pressurePlates.Count}"
        );

        OnPressurePlateCountChanged?.Invoke(activatedCount, pressurePlates.Count);

        if (activatedCount >= pressurePlates.Count)
        {
            //OnPlayerCompletedPuzzle?.Invoke();
            //OnPuzzleCompleted?.Invoke(PuzzleRoomType.PressurePlate);

            Debug.Log("Pressure plate puzzle completed");
        }
    }

    private bool CompletedCountingTypePuzzle(PuzzleRoomType type)
    {
        List<ObjectController> focus = null;
        switch (currentPuzzle)
        {
            case PuzzleRoomType.Lever:
                focus = levers;
                break;
            case PuzzleRoomType.PressurePlate:
                focus = pressurePlates;
                break;
        }

        if(focus == null)
            return false;

        foreach (var obj in focus)
        {
            if (!obj.IsActivated())
            {
                return false;
            }
        }

        Debug.Log("in CompletedLeverPuzzle");
        return true;

    }

    /*
    private void CheckButtonPuzzleOrder(ObjectController pressedButton)
    {
        if (currentPuzzle != PuzzleRoomType.Button)
            return;

        // Is this exactly the button we're expecting?
        if (pressedButton != buttons[nextButtonIndex])
        {
            FailButtonPuzzle();
            return;
        }

        nextButtonIndex++;

        // All four pressed correctly
        if (nextButtonIndex >= buttons.Count)
        {
            CompleteButtonPuzzle();
        }
    }
    */

    private void CheckButtonPuzzleOrder(ObjectController pressedButton)
    {
        if (currentPuzzle != PuzzleRoomType.Button)
            return;

        Debug.Log($"nextButtonIndex: {nextButtonIndex}, pressedButton: {pressedButton.transform.parent.name}");
        // wrong
        if (pressedButton != buttons[nextButtonIndex])
        {
            OnButtonPressResult?.Invoke(ButtonPressResult.Wrong);

            FailButtonPuzzle();
            return;
        }

        // correct
        nextButtonIndex++;

        // finished
        if (nextButtonIndex >= buttons.Count)
        {
            OnButtonPressResult?.Invoke(ButtonPressResult.Completed);

            CompleteButtonPuzzle();
            return;
        }

        // partial
        //OnButtonPressResult?.Invoke(ButtonPressResult.Correct);
    }

    private void HandleButtonExpired(ObjectController button)
    {
        if (currentPuzzle != PuzzleRoomType.Button)
            return;

        // If puzzle isn't already complete, timeout = fail
        if (nextButtonIndex < buttons.Count)
        {
            OnButtonPuzzleExpired?.Invoke();
            FailButtonPuzzle();
        }
    }

    private void FailButtonPuzzle()
    {
        Debug.Log("Button puzzle failed!");

        nextButtonIndex = 0;

        foreach (ObjectController button in buttons)
        {
            button.ResetButton();
        }
    }

    private void CompleteButtonPuzzle()
    {
        Debug.Log("Button puzzle completed!");

        //OnPlayerCompletedPuzzle?.Invoke();
        OnPuzzleCompleted?.Invoke(PuzzleRoomType.Button);

        nextButtonIndex = 0;
    }

    private void ShapeInserted(ObjectController shape)
    {
        if (currentPuzzle != PuzzleRoomType.SquareHole)
            return;

        if (squareHoleCompleted)
            return;

        insertedShapeCount++;

        Debug.Log($"Shape inserted: {shape.objectId} " + $"({insertedShapeCount}/{requiredShapes})");

        if (insertedShapeCount >= requiredShapes)
        {
            squareHoleCompleted = true;

            Debug.Log("Square hole puzzle completed!");

            OnPuzzleCompleted?.Invoke(PuzzleRoomType.SquareHole);
        }
    }

    // LEVER
    private int GetActivatedLeverCount()
    {
        int count = 0;

        foreach (ObjectController lever in levers)
        {
            if (lever.IsActivated())
            {
                count++;
            }
        }

        return count;
    }


    private int GetActivatedPressurePlateCount()
    {
        int count = 0;

        foreach (ObjectController plate in pressurePlates)
        {
            if (plate.IsActivated())
            {
                count++;
            }
        }

        return count;
    }



}


public enum ButtonPressResult
{
    Correct,
    Wrong,
    Completed
}
