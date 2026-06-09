using UnityEngine;

public class HandleIndicationHUD : MonoBehaviour
{
    [SerializeField] GameObject indicatorHUDs;
    [SerializeField] IndicatorInstance[] indicatorInstances;

    private void Start()
    {
        EventBus<OnShowIndicator>.OnEvent += HandleIndicator;
        HandleIndicator(new OnShowIndicator(false));
    }

    private void OnDestroy()
    {
        EventBus<OnShowIndicator>.OnEvent -= HandleIndicator;
    }

    private void HandleIndicator(OnShowIndicator evt)
    {
        indicatorHUDs.SetActive(evt.isActive);

        for (int i = 0; i < indicatorInstances.Length; i++)
        {
            bool active = indicatorInstances[i].indicatorType == evt.indicator;
            indicatorInstances[i].instance.SetActive(active);
        }
    }
}
