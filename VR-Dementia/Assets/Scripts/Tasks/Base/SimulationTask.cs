using System;
using UnityEngine;
using UnityEngine.Localization;

public class SimulationTask : MonoBehaviour
{
    [SerializeField] LocalizedString taskTitle;
    [SerializeField] LocalizedString taskDescription;
    public event Action onTaskFinished;

    public virtual void StartTask()
    {
        EventBus<OnUpdateTask>.Publish(new OnUpdateTask(taskTitle.GetLocalizedString(), taskDescription.GetLocalizedString()));
    }

    public virtual void FinishTask()
    {
        onTaskFinished?.Invoke();
    }
}
