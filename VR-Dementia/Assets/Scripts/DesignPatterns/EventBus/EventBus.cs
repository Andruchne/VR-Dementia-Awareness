using FMODUnity;
using System;
using UnityEngine;

/// <summary>
/// Key part of the project script architecture
/// It enables decoupled invoking and listening, for any event currently needed
/// </summary>

public abstract class Event { }

// To subscribe: EventBus<EventClass>.OnEvent += Function;
// To invoke event: EventBus<EventClass>.Publish(new EventClass())
public class EventBus<T> where T : Event
{
    public static event Action<T> OnEvent;

    public static void Publish(T pEvent)
    {
        OnEvent?.Invoke(pEvent);
    }
}

#region Controls

public class OnChangePalmMenuActive : Event
{
    // Trigger and set, to control whether palm menu should be visisble
    public OnChangePalmMenuActive(bool isActive) 
    { 
        this.isActive = isActive;
    }
    public bool isActive;
}

public class OnPalmMenuVisibilityChanged : Event
{
    // Notifies of current visibility state of the palm menu
    public OnPalmMenuVisibilityChanged(bool isVisible)
    {
        this.isVisible = isVisible;
    }
    public bool isVisible;
}

public class OnMoved : Event
{
    // Notifies when player has moved (used for input tutorial)
    public OnMoved() { }
}

public class OnTurned : Event
{
    // Notifies when player has turned (used for input tutorial)
    public OnTurned() { }
}

#endregion

#region JulietteTalk

public class OnJulietteTalk : Event
{
    // Used to make Juliette talk
    // This ensures the audio is coming from the right direction
    public OnJulietteTalk(EventReference phrase)
    {
        this.phrase = phrase;
    }
    public EventReference phrase;
}

public class OnJulietteFinishedTalk : Event
{
    public OnJulietteFinishedTalk() { }
}

#endregion

#region UI

public class OnTransitionScreen : Event
{
    // Triggered to transition screen to black
    public OnTransitionScreen(float targetPercent, float duration)
    {
        this.targetPercent = targetPercent;
        this.duration = duration;
    }
    public float targetPercent;
    public float duration;
}

#endregion

#region Tasks

public class OnUpdateTask : Event
{
    // Used to update the text on the handheld menu
    public OnUpdateTask() { }
}

public class OnStartSimulation : Event
{
    // Triggered when send button is pressed, in the play scene
    public OnStartSimulation() { }
}

public class OnEnterBuilding : Event
{
    // Triggered when player enters the building
    public OnEnterBuilding() { }
}

public class OnJulietteSitDown : Event
{
    // Triggered after Juliette sat down
    public OnJulietteSitDown() { }
}

public class OnSugarPlacedDown : Event
{
    // Triggered when sugar is placed infront of Juliette
    public OnSugarPlacedDown() { }
}

public class OnPlayerSitDown : Event
{
    // Triggered when player sits down (after sugar has been brought)
    public OnPlayerSitDown(bool isSitting) 
    {
        this.isSitting = isSitting;
    }

    public bool isSitting;
}

#endregion
