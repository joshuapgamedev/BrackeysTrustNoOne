using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TaskListManager : MonoBehaviour
{
    [SerializeField] private GameObject taskListBox;
    [SerializeField] private GameObject checkMark;

    [SerializeField] private TextMeshProUGUI taskText;
    [SerializeField] private TaskListSO taskListData;
 
    public static TaskListManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public void DisplayTask(string taskID)
    {
        foreach(var task in taskListData.tasks)
        {
            if (task.taskID != taskID)
                continue;

            taskText.text = task.taskText;
            taskListBox.SetActive(true);
        }
    }

    public void CompleteTask(string taskID)
    {
        checkMark.SetActive(true);
    }

    public void ClearTask()
    {
        taskListBox.SetActive(false);
        checkMark.SetActive(false);
        taskText.text = "";
    }
}
