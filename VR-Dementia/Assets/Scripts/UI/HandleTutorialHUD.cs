using UnityEngine;

public class HandleTutorialHUD : MonoBehaviour
{
    [SerializeField] GameObject tutorialHUDs;
    [SerializeField] TutorialInstance[] tutorialInstances;

    private void Start()
    {
        for (int i = 0; i < tutorialInstances.Length; i++)
        {
            tutorialInstances[i].instance.SetActive(false);
        }

        EventBus<OnShowTutorial>.OnEvent += HandleTutorial;
        HandleTutorial(new OnShowTutorial(false));
    }

    private void OnDestroy()
    {
        EventBus<OnShowTutorial>.OnEvent -= HandleTutorial;
    }

    private void HandleTutorial(OnShowTutorial evt)
    {
        tutorialHUDs.SetActive(evt.isActive);

        for (int i = 0; i < tutorialInstances.Length; i++)
        {
            bool active = tutorialInstances[i].tutorialType == evt.tutorial;
            tutorialInstances[i].instance.SetActive(active);
        }
    }
}
