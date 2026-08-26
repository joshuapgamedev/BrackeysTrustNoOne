using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tasks/Task List")]
public class TaskListSO : ScriptableObject
{
    public List<TaskData> tasks;
}

[System.Serializable]
public class TaskData
{
    public string taskID;

    [TextArea]
    public string taskText;
}