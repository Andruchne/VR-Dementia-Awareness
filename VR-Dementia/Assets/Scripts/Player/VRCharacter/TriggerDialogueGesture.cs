using UnityEngine;

public class TriggerDialogueGesture : MonoBehaviour
{
    private void Start()
    {
        EventBus<OnRequestTalk>.OnEvent += TriggerDialogue;
    }

    private void OnDestroy()
    {
        EventBus<OnRequestTalk>.OnEvent -= TriggerDialogue;
    }

    public void TriggerDialogue(OnRequestTalk evt)
    {

    }

    public void RequestTalk()
    {
        EventBus<OnRequestTalk>.Publish(new OnRequestTalk());
    }
}
