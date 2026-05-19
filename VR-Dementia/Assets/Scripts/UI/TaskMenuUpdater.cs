using TMPro;
using UnityEngine;

public class TaskMenuUpdater : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI description;

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
        title.text = task.taskTitle;
        description.text = task.taskDescription;
    }
}