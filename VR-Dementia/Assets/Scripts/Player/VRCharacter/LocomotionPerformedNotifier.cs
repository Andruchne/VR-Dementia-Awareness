using Oculus.Interaction.Locomotion;
using UnityEngine;

public class LocomotionPerformedNotifier : MonoBehaviour
{
    [Header("Move Locomotion")]
    [SerializeField] TeleportInteractor[] teleportLocomotion;
    [SerializeField] SlideLocomotionBroadcaster[] slideLocomotion;

    [Header("Turn Locomotion")]
    [SerializeField] TurnerEventBroadcaster[] controllerTurnLocomotion;
    [SerializeField] TurnLocomotionBroadcaster[] handTurnLocomotion;

    private void Start()
    {
        SetupEventListeners();
    }

    private void OnDestroy()
    {
        UnsubscribeEventListeners();
    }

    private void SetupEventListeners()
    {
        // For movement
        for (int i = 0; i < teleportLocomotion.Length; i++)
        {
            teleportLocomotion[i].WhenLocomotionPerformed += PlayerMoved;
        }
        for (int i = 0; i < slideLocomotion.Length; i++)
        {
            slideLocomotion[i].WhenLocomotionPerformed += PlayerMoved;
        }

        // For rotation
        for (int i = 0; i < controllerTurnLocomotion.Length; i++)
        {
            controllerTurnLocomotion[i].WhenLocomotionPerformed += PlayerRotated;
        }
        for (int i = 0; i < handTurnLocomotion.Length; i++)
        {
            controllerTurnLocomotion[i].WhenLocomotionPerformed += PlayerRotated;
        }
    }

    private void UnsubscribeEventListeners()
    {
        // For movement
        for (int i = 0; i < teleportLocomotion.Length; i++)
        {
            teleportLocomotion[i].WhenLocomotionPerformed -= PlayerMoved;
        }
        for (int i = 0; i < slideLocomotion.Length; i++)
        {
            slideLocomotion[i].WhenLocomotionPerformed -= PlayerMoved;
        }

        // For rotation
        for (int i = 0; i < controllerTurnLocomotion.Length; i++)
        {
            controllerTurnLocomotion[i].WhenLocomotionPerformed -= PlayerRotated;
        }
        for (int i = 0; i < handTurnLocomotion.Length; i++)
        {
            controllerTurnLocomotion[i].WhenLocomotionPerformed -= PlayerRotated;
        }
    }

    private void PlayerMoved(LocomotionEvent evt)
    {
        EventBus<OnMoved>.Publish(new OnMoved());
        Debug.LogWarning("Moved");
    }

    private void PlayerRotated(LocomotionEvent evt)
    {
        EventBus<OnTurned>.Publish(new OnTurned());
        Debug.LogWarning("Turned");
    }
}
