using UnityEngine;

public class GrabObjectChecker : MonoBehaviour
{
    [SerializeField] private TriggerReporter toArea;
    [SerializeField] private PhysicsProxyFollower grabbableObject;
    [SerializeField] private Transform door;
    [SerializeField] private Transform text;
    [SerializeField] private Transform grabText;

    private void Awake()
    {
        if (toArea != null) { toArea.onTriggerEntered += HandleObjectEnteredArea; }
    }

    private void OnDestroy()
    {
        if (toArea != null) { toArea.onTriggerEntered -= HandleObjectEnteredArea; }
    }

    private void Update()
    {
        if (grabbableObject.IsGrabbed() && !toArea.gameObject.activeSelf && door.gameObject.activeSelf)
        {
            toArea.gameObject.SetActive(true);
            text.gameObject.SetActive(true);
            grabText.gameObject.SetActive(false);
        }
        else if (!grabbableObject.IsGrabbed() && door.gameObject.activeSelf)
        {
            toArea.gameObject.SetActive(false);
            text.gameObject.SetActive(false);
            grabText.gameObject.SetActive(true);
        }
    }

    private void HandleObjectEnteredArea(Collider other)
    {
        // Check if the object that entered the trigger is specifically our grabbable object
        if (other.gameObject == grabbableObject.gameObject)
        {
            toArea.gameObject.SetActive(false);
            door.gameObject.SetActive(false);
            text.gameObject.SetActive(false);
            grabText.gameObject.SetActive(false);
        }
    }
}