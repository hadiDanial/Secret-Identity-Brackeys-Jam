using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CostumeChanger : MonoBehaviour
{
    [SerializeField] private List<OutfitPiece> heroOutfitPieces;
    [SerializeField] private AudioClip changingAudioClip;
    [SerializeField] private bool isCurrentlyChanging;
    [SerializeField, Tooltip("Amount of time between changing attempts (so the player doesn't spam it)")] private float cooldownTimer = 1f;
    private float changingTimer, currentCooldownTime = 0;
    private int index = 0;
    private void Update()
    {
        if (isCurrentlyChanging)
        {
            changingTimer += Time.deltaTime;
            Debug.Log("Changing, time: " + changingTimer);
            if(changingTimer >= heroOutfitPieces[index].changeTime)
            {
                StopChanging();
                if(heroOutfitPieces[index].normalOutfit != null)
                    heroOutfitPieces[index].normalOutfit?.SetActive(false);
                if (heroOutfitPieces[index].heroOutfit != null)
                    heroOutfitPieces[index].heroOutfit?.SetActive(true);
                index++;
                if(index >= heroOutfitPieces.Count)
                {
                    Debug.Log("WIN");
                    // TODO - Game manager, win screen
                }
            }
        }
        else
        {
            currentCooldownTime -= Time.deltaTime;
        }
    }

    public void StartChanging()
    {
        changingTimer = 0;
        Debug.Log("Started Changing");
        // Display UI
    }

    public void StopChanging()
    {
        changingTimer = 0;
        currentCooldownTime = cooldownTimer;
        isCurrentlyChanging = false;
        Debug.Log("Stopped Changing");
        // Hide UI
    }

    internal void ChangeInput(bool isPressed)
    {
        if (heroOutfitPieces == null || heroOutfitPieces.Count == 0 || index >= heroOutfitPieces.Count)
            return;
        if(!isCurrentlyChanging && isPressed && currentCooldownTime <= 0)
        {
            isCurrentlyChanging = isPressed;
            StartChanging();
        }
        else if(isCurrentlyChanging && !isPressed)
        {
            isCurrentlyChanging = isPressed;
            StopChanging();
        }
        else
        {
            isCurrentlyChanging = isPressed;
        }
    }
}
