using System;
using UnityEngine;
using UnityEngine.Localization;

public class SimulationTask : MonoBehaviour
{
    [SerializeField] LocalizedString taskTitle;
    [SerializeField] LocalizedString taskDescription;
    public event Action onTaskFinished;

    protected bool isActive;
    protected bool isFinished;

    public virtual void StartTask()
    {
        if (isActive) { return; }

        EventBus<OnUpdateTask>.Publish(new OnUpdateTask(taskTitle.GetLocalizedString(), taskDescription.GetLocalizedString()));
        isActive = true;
    }

    public virtual void FinishTask()
    {
        if (isFinished) { return; }

        onTaskFinished?.Invoke();
        isFinished = true;
        isActive = false;
    }
}
