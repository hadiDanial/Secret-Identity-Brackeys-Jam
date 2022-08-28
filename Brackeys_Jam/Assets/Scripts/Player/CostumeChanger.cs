using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class CostumeChanger : MonoBehaviour
{
    [SerializeField] private List<OutfitPiece> heroOutfitPieces;
    [SerializeField] private AudioClip changingAudioClip;
    [SerializeField] private bool isCurrentlyChanging;
    [SerializeField, Tooltip("Amount of time between changing attempts (so the player doesn't spam it)")] private float cooldownTimer = 1f;
    [SerializeField] private Image changeFillImage;
    [SerializeField] private RectTransform changeFillRect;

    private float changeTimer, currentCooldownTime = 0;
    private int index = 0;
    private PlayerController playerController;
    private bool canChange = true;
    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        changeFillImage.fillAmount = 0;
    }
    private void Update()
    {
        if (isCurrentlyChanging)
        {
            changeTimer += Time.deltaTime;
            float changeTime = heroOutfitPieces[index].changeTime;
            changeFillImage.fillAmount = Mathf.Clamp01(changeTimer / changeTime);
            changeFillImage.transform.parent.GetComponent<Canvas>().worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            //Debug.Log("Changing, time: " + changingTimer);
            if (changeTimer >= changeTime)
            {
                FinishChanging();                
            }
        }
        else
        {
            currentCooldownTime -= Time.deltaTime;
            //changeFillImage.fillAmount = 0;
            // We just finished changing, cooldown has finished
            if (!canChange && currentCooldownTime <= 0)
            {
                canChange = true;
            }
        }
    }

    public void StartChanging()
    {
        changeTimer = 0;
        Debug.Log("Started Changing");
        // Display UI
    }

    public void FinishChanging()
    {
        if(canChange)
        {
            StopChanging();
            if (heroOutfitPieces[index].normalOutfit != null)
                heroOutfitPieces[index].normalOutfit?.SetActive(false);
            if (heroOutfitPieces[index].heroOutfit != null)
                heroOutfitPieces[index].heroOutfit?.SetActive(true);
            playerController.SetDetectionAction(heroOutfitPieces[index].detectAction);
            DOTween.Sequence().Append(changeFillRect.DOScale(1.2f, 0.5f).SetEase(Ease.InElastic))
                .Append(changeFillRect.DOScale(1, 0.2f).SetEase(Ease.InElastic))
                .OnComplete(() => changeFillImage.fillAmount = 0).Play();                          
            index++;
            if (index >= heroOutfitPieces.Count)
            {

                GameManager.GetInstance().ChangeGameState(GameState.WON);
            }
            canChange = false;
        }
    }

    private void StopChanging()
    {
        changeTimer = 0;
        currentCooldownTime = cooldownTimer;
        isCurrentlyChanging = false;
        changeFillImage.fillAmount = 0;
        Debug.Log("Stopped Changing");
    }

    internal void ChangeInput(bool isPressed)
    {
        if (heroOutfitPieces == null || heroOutfitPieces.Count == 0 || index >= heroOutfitPieces.Count)
            return;
        // We just finished changing, button is still pressed.
        if ((!canChange && isPressed) || playerController.IsCrouched)
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

    public bool IsChanging()
    {
        return isCurrentlyChanging;
    }

    public bool CanChange()
    {
        return canChange;
    }
}
