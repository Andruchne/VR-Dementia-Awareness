using UnityEngine;

public class EnterHouseTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        EventBus<OnEnterBuilding>.Publish(new OnEnterBuilding());
        gameObject.SetActive(false);
    }
}
