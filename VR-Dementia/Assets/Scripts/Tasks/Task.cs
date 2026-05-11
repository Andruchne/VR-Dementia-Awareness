using System;
using UnityEngine;

public class SimulationTask : MonoBehaviour
{
    [SerializeField] private string taskTitle;
    [TextArea(3,3)][SerializeField] private string taskDescription;
    public event Action onTaskFinished;

    public virtual void StartTask() 
    {
        EventBus<OnUpdateTask>.Publish(new OnUpdateTask(taskTitle, taskDescription));
    }

    public virtual void FinishTask() 
    {
        onTaskFinished?.Invoke();
    }
}
