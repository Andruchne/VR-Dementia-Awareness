using UnityEngine;

public class GrabSugarTrigger : MonoBehaviour
{
    [SerializeField] GameObject grabTutorialWindow;
    [SerializeField] PhysicsProxyFollower sugarGrabbable;
    [SerializeField] GameObject placeSugarHere;

    private MeshRenderer triggerArea;

    private bool isActive;
    private bool userInside;
    private bool showIndicator;
    private bool wasGrabbed;

    private void Start()
    {
        triggerArea = GetComponent<MeshRenderer>();
        SetActive(false);

        EventBus<OnSugarPlacedDown>.OnEvent += SugarPlaced;
    }

    private void OnDestroy()
    {
        EventBus<OnSugarPlacedDown>.OnEvent -= SugarPlaced;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") { userInside = true; }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player") { userInside = false; }
        
    }

    public void SetActive(bool isActive)
    {
        if (triggerArea == null || grabTutorialWindow == null) { return; }
        this.isActive = isActive;
        showIndicator = isActive;

        // Turn off everything if disabled
        if (!isActive)
        {
            triggerArea.enabled = false;
            grabTutorialWindow.SetActive(false);
            placeSugarHere.SetActive(false);
        }
        /*
        else
        {
            if (userInside)
            {
                triggerArea.enabled = false;
                grabTutorialWindow.SetActive(true);
            }
            else
            {
                triggerArea.enabled = true;
                grabTutorialWindow.SetActive(false);
            }
        }
        */
    }

    private void CheckCurrentlyVisible()
    {
        if (!isActive) { return; }

        // Toggle the area visibility, based on if the player is inside
        if (showIndicator) 
        { 
            triggerArea.enabled = !userInside;
            grabTutorialWindow.SetActive(userInside);
        }

        if (sugarGrabbable.IsGrabbed())
        {
            placeSugarHere.SetActive(true);

            showIndicator = false;
            triggerArea.enabled = false;
            grabTutorialWindow.SetActive(false);
            wasGrabbed = true;
        }
        else if (!sugarGrabbable.IsGrabbed())
        {
            placeSugarHere.SetActive(false);

            if (wasGrabbed && userInside)
            {
                showIndicator = true;
                wasGrabbed = false;
            }
        }
    }

    private void Update()
    {
        CheckCurrentlyVisible();
    }

    private void SugarPlaced(OnSugarPlacedDown evt)
    {
        // We could technically also destroy the object, but this prevents potential null reference issues in other scripts
        triggerArea.enabled = false;
        grabTutorialWindow.SetActive(false);
        isActive = false;
    }
}
