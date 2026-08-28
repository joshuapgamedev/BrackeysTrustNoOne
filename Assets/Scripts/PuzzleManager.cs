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

    public static event System.Action OnPlayerCompletedPuzzle;
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
        if (CompletedCountingTypePuzzle(currentPuzzle))
        {
            OnPlayerCompletedPuzzle?.Invoke();
            Debug.Log("Lever completed");
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
    private void HandleButtonExpired(ObjectController button)
    {
        if (currentPuzzle != PuzzleRoomType.Button)
            return;

        // If puzzle isn't already complete, timeout = fail
        if (nextButtonIndex < buttons.Count)
        {
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

        OnPlayerCompletedPuzzle?.Invoke();

        nextButtonIndex = 0;
    }

    private void ShapeInserted(ObjectController shape)
    {
        if (currentPuzzle != PuzzleRoomType.SquareHole)
            return;

        insertedShapeCount++;

        Debug.Log($"Shape inserted: {shape.objectId} " + $"({insertedShapeCount}/3)");

        if (insertedShapeCount >= requiredShapes)
        {
            OnPlayerCompletedPuzzle?.Invoke();

            Debug.Log("Square hole puzzle completed!");
        }
    }

}
