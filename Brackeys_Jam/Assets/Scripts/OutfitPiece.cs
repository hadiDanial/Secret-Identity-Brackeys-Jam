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
    //[SerializeField] public bool hasChanged;
}
