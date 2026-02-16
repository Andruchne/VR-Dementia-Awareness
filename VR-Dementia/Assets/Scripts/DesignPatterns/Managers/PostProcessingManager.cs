using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PostProcessingManager : MonoBehaviour
{
    public VolumeCollection volumeCollection;
    [Space]
    [SerializeField]
    private float transitionTime = 2.0f;

    // For management
    private Dictionary<Mood, GameObject> _volumeInstances = new Dictionary<Mood, GameObject>();

    private float _transitionSpeed;
    private Mood _currentMood = Mood.Neutral;

    private void Awake()
    {
        if (GameManager.Instance != null) { GameManager.Instance.OnInputSetup += SubscribeInput; }
    }

    private void Start()
    {
        // Instantiate all volumes 
        for (int i = 0; i < volumeCollection.entries.Count; i++)
        {
            _volumeInstances.Add(volumeCollection.entries[i].mood, Instantiate(volumeCollection.entries[i].volume, Vector3.zero, Quaternion.identity));
        }

        _transitionSpeed = 1.0f / transitionTime;
    }

    public void SwitchMood(Mood mood)
    {

    }

    private void SubscribeInput()
    {
        GameManager.Instance.gameInput.Debug.SwitchLanguage.performed += ToggleTransition;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnInputSetup -= SubscribeInput;
            GameManager.Instance.gameInput.Debug.SwitchLanguage.performed -= ToggleTransition;
        }
    }

    private void ToggleTransition(InputAction.CallbackContext context)
    {
        
    }
}
