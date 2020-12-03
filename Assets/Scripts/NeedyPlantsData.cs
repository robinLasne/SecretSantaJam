using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Needy Plants Data")]
public class NeedyPlantsData : SingletonScriptableObject<NeedyPlantsData>
{
    [System.Serializable]
    public class LeafType {
        public int type;
        public Sprite leafSprite;
    }

    public LeafType[] data;
}
