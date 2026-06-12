using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleTutorialComponents : MonoBehaviour
{
    [SerializeField] GameObject tutorialHUDs;
    [SerializeField] TutorialInstance[] tutorialInstances;

    private Dictionary<TutorialType, TutorialInstance> tutorials;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);

        tutorials = new Dictionary<TutorialType, TutorialInstance>();

        for (int i = 0; i < tutorialInstances.Length; i++)
        {
            tutorialInstances[i].hudInstance.SetActive(false);

            for (int a = 0; a < tutorialInstances[i].movementComponent.Length; a++)
            {
                tutorialInstances[i].movementComponent[a].SetActive(false);
            }

            tutorials.Add(tutorialInstances[i].tutorialType, tutorialInstances[i]);
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
        if (evt.tutorial == TutorialType.None) { return; }

        for (int i = 0; i < tutorialInstances.Length; i++)
        {
            bool active = tutorialInstances[i].tutorialType == evt.tutorial;
            tutorialInstances[i].hudInstance.SetActive(active);
        }
        
        for (int i = 0; i < tutorials[evt.tutorial].movementComponent.Length; i++)
        {
            tutorials[evt.tutorial].movementComponent[i].SetActive(true);
        }
    }
}
