using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct intArray {
    public int[] array;

    public int this[int i] {
        get {
            return array[i];
        }
        set {
            array[i] = value;
        }
    }
}

[CreateAssetMenu(menuName ="ScriptableObjects/Level Data")]
public class LevelStartData : ScriptableObject
{
    public int hexagonRadius;
    [HideInInspector]
    public intArray[] cells;
}
