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

#region Trigger Actions

// Events for triggering certain Actions from object/characters
public class OnGrandmaDance : Event
{
    // This is just a placeholder, you won't take damage from grandma
    public OnGrandmaDance(float damage)
    {
        this.damage = damage;
    }
    public float damage;
}

#endregion
