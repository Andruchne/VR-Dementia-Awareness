using UnityEngine;

public class GrabSugarTrigger : MonoBehaviour
{
    [SerializeField] GameObject grabTutorialWindow;
    [SerializeField] MeshRenderer triggerArea;

    private void Start()
    {
        triggerArea = GetComponent<MeshRenderer>();

        if (triggerArea == null) { Destroy(gameObject); }
        if (grabTutorialWindow != null) { grabTutorialWindow.SetActive(false); }
    }

    private void OnTriggerEnter(Collider other)
    {
        triggerArea.enabled = false;
        grabTutorialWindow.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        triggerArea.enabled = true;
        grabTutorialWindow.SetActive(false);
    }
}
