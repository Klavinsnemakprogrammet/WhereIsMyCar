using UnityEngine;
using UnityEngine.SceneManagement;

public class LivesScript : MonoBehaviour
{
    public static LivesScript Instance;
    public static bool isGameOver = false;

    [Header("Lives")]
    public int maxLives = 3;
    public float hitCooldown = 1.5f;   // seconds after a hit where more hits are ignored

    [Header("UI")]
    public GameObject[] hearts;        // drag the 3 heart objects from the Hierarchy, left to right
    public GameObject gameOverPanel;   // optional

    private int lives;
    private float lastHitTime = -999f;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        isGameOver = false;
        lives = maxLives;
        lastHitTime = -999f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (hearts == null || hearts.Length == 0)
            Debug.LogWarning("LivesScript: Hearts array is empty!", this);

        UpdateHearts();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool LoseLife()
    {
        if (isGameOver) return false;
        if (Time.unscaledTime - lastHitTime < hitCooldown) return false;

        lastHitTime = Time.unscaledTime;
        lives = Mathf.Max(0, lives - 1);
        Debug.Log("Lives left: " + lives, this);

        UpdateHearts();

        if (lives <= 0)
            GameOver();

        return true;
    }

    void UpdateHearts()
    {
        if (hearts == null) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null)
            {
                Debug.LogWarning("Heart slot " + i + " is empty!", this);
                continue;
            }
            hearts[i].SetActive(i < lives);
        }
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over!", this);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // Hook to the Restart button's OnClick
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}