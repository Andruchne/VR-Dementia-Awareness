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

#region Tasks

public class OnUpdateTask : Event
{
    // Used to update the text on the handheld menu
    public OnUpdateTask(string taskTitle, string taskDescription) 
    {
        this.taskTitle = taskTitle;
        this.taskDescription = taskDescription;
    }
    public string taskTitle;
    public string taskDescription;
}

public class OnStartSimulation : Event
{
    // Triggered when send button is pressed, in the play scene
    public OnStartSimulation() { }
}

#endregion
