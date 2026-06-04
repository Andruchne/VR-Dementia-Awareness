using UnityEngine;

public class GrabSugarTrigger : MonoBehaviour
{
    [SerializeField] GameObject grabTutorialWindow;
    [SerializeField] PhysicsProxyFollower sugarGrabbable;
    [SerializeField] GameObject placeSugarHere;

    private MeshRenderer triggerArea;
    private bool stopChecking;
    private bool showIndication;

    private bool userInside;

    private void Start()
    {
        triggerArea = GetComponent<MeshRenderer>();

        if (triggerArea != null) { triggerArea.enabled = false; }
        if (grabTutorialWindow != null) { grabTutorialWindow.SetActive(false); }

        EventBus<OnSugarPlacedDown>.OnEvent += SugarPlaced;
    }

    private void OnDestroy()
    {
        EventBus<OnSugarPlacedDown>.OnEvent -= SugarPlaced;
    }

    private void Update()
    {
        if (stopChecking) { return; }

        if (sugarGrabbable.IsGrabbed())
        {
            triggerArea.enabled = false;
            grabTutorialWindow.SetActive(false);
            placeSugarHere.SetActive(true);
        }
        else if (!triggerArea.enabled && !sugarGrabbable.IsGrabbed() && showIndication)
        {
            grabTutorialWindow.SetActive(true);
            placeSugarHere.SetActive(false);
        }
        else if (sugarGrabbable.IsGrabbed() && !userInside && showIndication)
        {
            showIndication = false;
            grabTutorialWindow.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (showIndication) { triggerArea.enabled = false; }
            grabTutorialWindow.SetActive(true);
            userInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            if (showIndication) { triggerArea.enabled = true; }
            grabTutorialWindow.SetActive(false);
            userInside = false;
        }
    }

    private void SugarPlaced(OnSugarPlacedDown evt)
    {
        // We could technically also destroy the object, but this prevents potential null reference issues in other scripts
        stopChecking = true;
        triggerArea.enabled = false;
        grabTutorialWindow.SetActive(false);
    }

    public void ShowPosIndication()
    {
        showIndication = true;
        if (userInside) { triggerArea.enabled = false; }
        else { triggerArea.enabled = true; }
    }
}
