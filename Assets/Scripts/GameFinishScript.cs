using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameFinishScript : MonoBehaviour
{
    [Header("Victory Screen Objects (hidden at start, shown on victory)")]
    [Tooltip("Drag in StarBackground, StarPlace_1, StarPlace_2, StarPlace_3, Restart, Home and TimeField")]
    public GameObject[] victoryObjects;

    [Header("Time")]
    public TMP_Text timeText;            // the "Time" TMP text inside TimeField

    [Header("Existing Stars (drag your 3 star Images here, left to right)")]
    public Image[] starImages;           // StarPlace_1, StarPlace_2, StarPlace_3

    [Header("Star Sprites (assign from your Assets folder)")]
    public Sprite filledStarSprite;      // shown on the stars the player earned
    public Sprite emptyStarSprite;       // optional - if empty, the star's original sprite is kept

    [Header("Buttons")]
    public Button restartButton;         // reloads the city scene
    public Button homeButton;            // goes to the start scene
    public string citySceneName = "CityScene";
    public string startSceneName = "StartScene";

    [Header("Time Thresholds (seconds)")]
    public float threeStarTime = 60f;    // finish in <= 60s  -> 3 stars
    public float twoStarTime = 120f;     // finish in <= 120s -> 2 stars, slower -> 1 star

    [Header("Game Settings")]
    public int totalCars = 12;           // how many cars must be placed to win

    [Header("Optional")]
    public AudioSource audioSource;
    public AudioClip victorySound;

    private Sprite[] originalSprites;
    private float startTime;
    private float finalTime;
    private int carsPlaced = 0;
    private bool gameFinished = false;

    public bool IsFinished { get { return gameFinished; } }

    void Awake()
    {
        // Remember what each star looks like now, so unearned stars can keep that look
        originalSprites = new Sprite[starImages.Length];
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
                originalSprites[i] = starImages[i].sprite;
        }

        // Hide everything that belongs to the victory screen
        SetVictoryObjectsActive(false);

        // Hook up the buttons (no need to set OnClick in the Inspector)
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (homeButton != null)
            homeButton.onClick.AddListener(GoHome);
    }

    void Start()
    {
        startTime = Time.time;
    }

    void SetVictoryObjectsActive(bool active)
    {
        foreach (GameObject obj in victoryObjects)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }

    // Call this every time a car is placed correctly
    public void CarPlaced()
    {
        if (gameFinished) return;

        carsPlaced++;
        if (carsPlaced >= totalCars)
            ShowVictory();
    }

    void ShowVictory()
    {
        gameFinished = true;
        finalTime = Time.time - startTime;

        if (timeText != null)
            timeText.text = FormatTime(finalTime);

        UpdateStars(GetStarCount(finalTime));
        SetVictoryObjectsActive(true);

        if (audioSource != null && victorySound != null)
            audioSource.PlayOneShot(victorySound);
    }

    int GetStarCount(float seconds)
    {
        if (seconds <= threeStarTime) return 3;
        if (seconds <= twoStarTime) return 2;
        return 1;
    }

    // Swaps the sprites on the stars that already exist in the scene
    void UpdateStars(int earned)
    {
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;

            if (i < earned)
                starImages[i].sprite = filledStarSprite;
            else
                starImages[i].sprite = emptyStarSprite != null ? emptyStarSprite : originalSprites[i];
        }
    }

    string FormatTime(float seconds)
    {
        int total = Mathf.FloorToInt(seconds);
        int h = total / 3600;
        int m = (total % 3600) / 60;
        int s = total % 60;
        return string.Format("{0:00}:{1:00}:{2:00}", h, m, s);
    }

    void ResetStatics()
    {
        GameObjectsScript.isDragging = false;
        GameObjectsScript.lastDragged = null;
    }

    // Restart button: loads the city scene again
    public void RestartGame()
    {
        ResetStatics();
        SceneManager.LoadScene(citySceneName);
    }

    // Home button: loads the start scene
    public void GoHome()
    {
        ResetStatics();
        SceneManager.LoadScene(startSceneName);
    }
}