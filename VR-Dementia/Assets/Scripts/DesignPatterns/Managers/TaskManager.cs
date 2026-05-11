using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    // Using GameObject, since interfaces can't be serialized
    [SerializeField] GameObject[] taskInstances;
    private List<SimulationTask> tasks = new List<SimulationTask>();

    private int currentTaskIndex;

    private void Start()
    {
        for (int i = 0; i < taskInstances.Length; i++)
        {
            if (taskInstances[i].TryGetComponent<SimulationTask>(out SimulationTask task))
            {
                tasks.Add(task);
                tasks[i].onTaskFinished += TriggerNextTask;
            }
        }

        if (tasks.Count > 0)
        {
            tasks[0].StartTask();
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < tasks.Count; i++)
        {
            tasks[i].onTaskFinished -= TriggerNextTask;
        }
    }

    private void TriggerNextTask()
    {
        currentTaskIndex++;
        tasks[currentTaskIndex].StartTask();
    }
}
