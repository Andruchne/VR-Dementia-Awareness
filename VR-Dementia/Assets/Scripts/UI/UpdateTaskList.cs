using UnityEngine;

public class UpdateTaskList : MonoBehaviour
{
    [SerializeField] TaskLine[] tasks;
    [SerializeField] Color inactiveColor;
    [SerializeField] Color activeColor;

    private int currentIndex;

    private void Start()
    {
        EventBus<OnUpdateTask>.OnEvent += UpdateTask;
    }

    private void OnDestroy()
    {
        EventBus<OnUpdateTask>.OnEvent -= UpdateTask;
    }

    private void UpdateTask(OnUpdateTask evt)
    {
        if (currentIndex >= tasks.Length) { return; }

        Debug.LogWarning(currentIndex);
        if (currentIndex - 1 >= 0)
        {
            tasks[currentIndex - 1].title.color = inactiveColor;
            tasks[currentIndex - 1].description.color = inactiveColor;
            Debug.LogWarning("deactivatePrevious");
        }

        tasks[currentIndex].title.color = activeColor;
        tasks[currentIndex].description.color = activeColor;

        currentIndex++;
    }
}
