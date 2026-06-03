using UnityEngine;

public class SitDownNotify : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        EventBus<OnPlayerSitDown>.Publish(new OnPlayerSitDown(true));
    }

    private void OnTriggerExit(Collider other)
    {
        EventBus<OnPlayerSitDown>.Publish(new OnPlayerSitDown(false));
    }
}
