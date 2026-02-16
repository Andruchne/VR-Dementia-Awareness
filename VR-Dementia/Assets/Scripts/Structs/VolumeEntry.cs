using UnityEngine;

/// <summary>
/// This class is used similar to a struct
/// It is not defined as a struct however, since modifying it would only yield a copy, instead of a reference
/// This avoids the issue of having to write extensive replacing logic in VolumeCollectionEditor.cs
/// </summary>

public class VolumeEntry
{
    public Mood mood;
    public GameObject volume;
}
