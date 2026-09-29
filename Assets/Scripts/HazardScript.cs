using System.Collections.Generic;
using UnityEngine;

// Marks an object as dangerous. Registers itself so spawned clouds/planes are found automatically.
public class HazardScript : MonoBehaviour
{
    public static readonly List<HazardScript> All = new List<HazardScript>();

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }
}