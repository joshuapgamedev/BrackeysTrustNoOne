using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private List<ObjectController> levers;
    [SerializeField] private List<ObjectController> pressurePlates;

    private PuzzleRoomType currentPuzzle;


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
    }

    private void OnDisable()
    {
        NarratorZone.OnPlayerEnterPuzzle -= ChoooseRoom;
        PlayerController.OnPlayerToggleLever -= CheckPuzzle;
        ObjectController.OnActivatePressurePlate -= CheckPuzzle;
    }

    private void ChoooseRoom(PuzzleRoomType puzzleType)
    {
        currentPuzzle = puzzleType;

        switch (currentPuzzle)
        {
            case PuzzleRoomType.Lever:
                
                return;
            default:
                break;
        }
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

}
