using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct OutfitPiece
{
    [SerializeField] public GameObject normalOutfit;
    [SerializeField] public GameObject heroOutfit;
    [SerializeField] public float changeTime;
    [SerializeField, Tooltip("Special behavior if the player is detected at this stage.\nOverrides the previous detect action.")]
    public DetectAction detectAction;
}


