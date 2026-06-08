using TMPro;
using UnityEngine;

public class TaskMenuUpdater : MonoBehaviour
{
    private int currentIndex;

    private void Awake()
    {
        EventBus<OnUpdateTask>.OnEvent += UpdateTask;
    }

    private void OnDestroy()
    {
        EventBus<OnUpdateTask>.OnEvent -= UpdateTask;
    }

    private void UpdateTask(OnUpdateTask task)
    {
        currentIndex++;
    }
}