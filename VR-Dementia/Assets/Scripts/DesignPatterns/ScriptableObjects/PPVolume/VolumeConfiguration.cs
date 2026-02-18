using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VolumeConfiguration", menuName = "Scriptable Objects/VolumeConfiguration")]
public class VolumeConfiguration : ScriptableObject
{
    public List<VolumeConfig> configs = new List<VolumeConfig>();
}