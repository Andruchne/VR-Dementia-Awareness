using UnityEngine;

/// <summary>
/// Used to hide a list of gameObjects, when entering a trigger collider.
/// Mainly used for quick iterations
/// </summary>
public class HideTrigger : MonoBehaviour
{
    [SerializeField] private GameObject[] hideGameObjects;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            foreach (GameObject gameObject in hideGameObjects)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
