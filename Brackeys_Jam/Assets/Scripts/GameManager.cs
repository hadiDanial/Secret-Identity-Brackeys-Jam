using DG.Tweening;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float gameTime;
    [SerializeField] private GameState currentGameState = GameState.NOT_STARTED;

    [Header("UI")]
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private Button restartBtn, exitBtn;
    [SerializeField] private Animator detectionStatusAnimator;
    [SerializeField] private Image detectionStatusImage;
    [SerializeField] private Color normalStatusColor = new Color(43, 43, 43);
    [SerializeField] private Color followStatusColor = new Color(229, 117, 0);
    [SerializeField] private Color failStatusColor = new Color(198, 12, 12);
    [Header("UI - Timer")]
    [SerializeField] private CanvasGroup timerCanvasGroup;
    [SerializeField] private RectTransform timerRectTransform;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float timerFadeInVerticalOffset = 500f;
    [SerializeField] private Vector2 timerPosition = new Vector2(0,-110);
    [Header("UI - Screens")] 
    [SerializeField] private CanvasGroup screenCanvasGroup;
    [SerializeField] private RectTransform screenRectTransform;
    [SerializeField] private List<Image> startImages;
    [SerializeField] private Image pauseImage;
    [SerializeField] private Image winImage;
    [SerializeField] private Image loseDetectedImage;
    [SerializeField] private Image loseOutOfTimeImage;
    [SerializeField] private float screenFadeInVerticalOffset = -1500f;
    [Header("UI - Other")]
    [SerializeField] private CanvasGroup blackFadeCanvasGroup;
    [SerializeField] private CanvasGroup interactCanvasGroup;
    [Header("Game Done Audio Clips")]
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip gameWonClip;
    
    private static GameManager instance;

    private float currentTime;
    private bool hasStartedGame = false, isPaused = false;
    private bool gameIsDone;
    private List<Image> images;
    private PlayerController playerController;
    private bool isDetected;
    private int startImagesIndex = 0;
    private bool isChangingScreen;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(this);
        }
    }
    private void Start()
    {
        blackFadeCanvasGroup.alpha = 1;
        blackFadeCanvasGroup.DOFade(0, 1).SetUpdate(true).OnComplete(()=> ActivateScreen(startImages[0]));
        playerController = FindObjectOfType<PlayerController>();
        images = new List<Image>();
        images.Add(pauseImage);
        images.Add(winImage);
        images.Add(loseDetectedImage);
        images.Add(loseOutOfTimeImage);
        screenCanvasGroup.alpha = 1;
        timerCanvasGroup.alpha = 0;
        interactCanvasGroup.alpha = 0;
        DeactivateAllScreens();
        restartBtn.gameObject.SetActive(false);
        exitBtn.gameObject.SetActive(false);
        Time.timeScale = 0;        
        playerController.LockCamera(true);        
        
        SetImageDetectionColor(normalStatusColor);
        NextScreen();
        if (currentGameState == GameState.IN_PROGRESS)
            StartGame();

    }
    public static GameManager GetInstance()
    {
        return instance;
    }

    public void ChangeGameState(GameState gameState)
    {
        currentGameState = gameState;
        switch (gameState)
        {
            case GameState.IN_PROGRESS:
                StartGame();
                break;
            case GameState.PAUSED:
                PauseGame();
                break;
            case GameState.WON:
                Win();
                break;
            case GameState.LOST_DETECTED:
                GameOver();
                break;
            case GameState.LOST_OUT_OF_TIME:
                GameOver();
                break;
            case GameState.NOT_STARTED:
                break;
            default:
                break;
        }
    }
    public void OnStartPressed()
    {
        switch (currentGameState)
        {
            case GameState.NOT_STARTED:
                if(!isChangingScreen)
                NextScreen();
                break;           
            case GameState.PAUSED:
                ResumeGame();
                break;
            case GameState.WON:
                RestartLevel();
                break;
            case GameState.LOST_DETECTED:
                RestartLevel();
                break;
            case GameState.LOST_OUT_OF_TIME:
                RestartLevel();
                break;
            default:
                break;
        }

    }

    private void NextScreen()
    {        
        if (startImagesIndex == startImages.Count)
        {
            StartGame();
        }
        else
        {
            if (startImagesIndex > 0)
                startImages[startImagesIndex - 1].gameObject.SetActive(false);
            startImages[startImagesIndex].gameObject.SetActive(true);
            ActivateScreen(startImages[startImagesIndex]);
            startImagesIndex++;            
        }
    }

    public void RestartLevel()
    {
        Sequence sequence = DOTween.Sequence();
        blackFadeCanvasGroup.alpha = 0;
        sequence.Append(blackFadeCanvasGroup.DOFade(1, 1)).SetUpdate(true).
            OnComplete(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));               
    }

    internal void SetDetected(bool isDetected)
    {
        this.isDetected = isDetected;
    }

    public void OnPausePressed()
    {
        switch (currentGameState)
        {
            case GameState.NOT_STARTED:
                if(!isChangingScreen)
                StartGame();
                break;
            case GameState.IN_PROGRESS:
                PauseGame();
                break;
            case GameState.PAUSED:
                ResumeGame();
                break;           
            default:
                break;
        }
    }

    private void Update()
    {
        if(!isPaused && hasStartedGame)
        {
            currentTime -= Time.deltaTime;
            detectionStatusAnimator.SetBool("Detected", isDetected);
            if(currentTime <= 0)
            {
                currentTime = 0;
                currentGameState = GameState.LOST_OUT_OF_TIME;
                GameOver();
            }
            timerText.text = String.Format("Time: {0:0.00}", currentTime);
        }
    }


    public void Win()
    {
        if (!gameIsDone)
        {
            if (gameWonClip != null)
                AudioSource.PlayClipAtPoint(gameWonClip, transform.position, 0.9f);
            gameIsDone = true;
            isPaused = true;
            restartBtn.gameObject.SetActive(true);
            exitBtn.gameObject.SetActive(true);
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;            
            DOTween.Sequence().AppendInterval(2).SetUpdate(true).OnComplete(() => ActivateScreen(winImage)).Play();            
        }
    }
    private void GameOver()
    {
        if(!gameIsDone)
        {
            if(gameOverClip != null)
                AudioSource.PlayClipAtPoint(gameOverClip, transform.position, 0.9f);
            gameIsDone = true;
            isPaused = true;
            restartBtn.gameObject.SetActive(true);
            exitBtn.gameObject.SetActive(true);
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            if (currentGameState == GameState.LOST_OUT_OF_TIME)
            {
                DOTween.Sequence()
                    .Append(timerRectTransform.DOLocalMove(Vector2.zero, 1f, false).SetEase(Ease.InBounce))
                    .Join(timerRectTransform.DOScale(1.5f, 1).SetEase(Ease.InElastic))
                    .Join(timerText.DOColor(Color.red,1))
                    .AppendInterval(2)
                    .SetUpdate(true)
                    .OnComplete(()=> ActivateScreen(loseOutOfTimeImage))                    
                    .Play();
                
            }
            else if (currentGameState == GameState.LOST_DETECTED)
            {
                DOTween.Sequence().AppendInterval(2).SetUpdate(true).OnComplete(() => ActivateScreen(loseDetectedImage)).Play();
            }
        }
    }

    public void StartGame()
    {
        foreach (Image image in startImages)
        {
            image.gameObject.SetActive(false);
        }

        startImagesIndex = startImages.Count;
        hasStartedGame = true;
        isPaused = false;
        currentTime = gameTime;
        Time.timeScale = 1;
        currentGameState = GameState.IN_PROGRESS;
        playerController.LockCamera(false);
        ActivateTimer();
    }

    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;
        currentGameState = GameState.PAUSED;
        playerController.LockCamera(true);
        ActivateScreen(pauseImage);
    }


    private void ResumeGame()
    {
        currentGameState = GameState.IN_PROGRESS;
        isPaused = false;
        Time.timeScale = 1;
        playerController.LockCamera(false);
        ActivateTimer();
    }

    private void ActivateTimer()
    {
        FadeInCanvas(timerCanvasGroup, timerRectTransform, timerFadeInVerticalOffset, timerPosition);
        FadeOutCanvas(screenCanvasGroup, screenRectTransform, screenFadeInVerticalOffset);
    }

    private void ActivateScreen(Image image)
    {
        EnableImageObject(image);
        FadeOutCanvas(timerCanvasGroup, timerRectTransform, timerFadeInVerticalOffset);
        FadeInCanvas(screenCanvasGroup, screenRectTransform, screenFadeInVerticalOffset, Vector2.zero);
    }

    internal void SetDetectionAction(DetectAction detectAction)
    {
        switch (detectAction)
        {
            case DetectAction.NORMAL:
                SetImageDetectionColor(normalStatusColor);
                break;
            case DetectAction.FOLLOW:
                SetImageDetectionColor(followStatusColor);
                break;
            case DetectAction.FAIL:
                SetImageDetectionColor(failStatusColor);
                break;
            default:
                break;
        }
    }

    private void SetImageDetectionColor(Color color)
    {        
        detectionStatusImage.DOColor(color, 1);
    }

    private void EnableImageObject(Image image)
    {
        DeactivateAllScreens();
        image.transform.gameObject.SetActive(true);
    }

    private void DeactivateAllScreens()
    {
        foreach (Image image in images)
        {
            image.transform.gameObject.SetActive(false);
        }
    }

    internal void DisplayInteractUI(bool v)
    {
        if (v)
            interactCanvasGroup.DOFade(1, 0.5f).SetEase(Ease.OutBack);
        else
            interactCanvasGroup.DOFade(0, 0.5f).SetEase(Ease.OutBack);
    }



    // UI

    public void FadeInCanvas(CanvasGroup canvasGroup, RectTransform rectTransform, float verticalOffset, Vector2 finalPosition)
    {
        isChangingScreen = true;
        canvasGroup.alpha = 0;
        rectTransform.transform.localPosition = new Vector3(0, verticalOffset, 0);
        rectTransform.DOAnchorPos(finalPosition, fadeTime, false).SetEase(Ease.OutElastic).SetUpdate(true).OnComplete(()=> isChangingScreen = false);
        canvasGroup.DOFade(1, fadeTime).SetUpdate(true);
    }
    public void FadeOutCanvas(CanvasGroup canvasGroup, RectTransform rectTransform, float verticalOffset)
    {
        isChangingScreen = true;
        canvasGroup.alpha = 1;
        rectTransform.transform.localPosition = Vector2.zero;
        rectTransform.DOAnchorPos(new Vector3(0, verticalOffset, 0), fadeTime, false).SetEase(Ease.InOutQuint).SetUpdate(true).OnComplete(() => isChangingScreen = false);
        canvasGroup.DOFade(0, fadeTime).SetUpdate(true);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
