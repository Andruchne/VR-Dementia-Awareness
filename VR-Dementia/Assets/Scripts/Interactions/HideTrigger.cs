using UnityEngine;

public class HideTrigger : MonoBehaviour
{
    [SerializeField] private GameObject[] hideGameObjects;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            foreach (GameObject gO in hideGameObjects)
            {
                gO.SetActive(false);
            }
        }
    }
}
