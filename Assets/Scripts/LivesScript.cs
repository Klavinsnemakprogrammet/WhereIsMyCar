using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LivesScript : MonoBehaviour
{
    public int maxLives = 3;

    [Header("UI")]
    public Image[] hearts;          // drag your 3 heart Images here, left to right
    public Sprite fullHeart;        // optional: if empty, lost hearts are just hidden
    public Sprite emptyHeart;       // optional
    public GameObject gameOverPanel; // optional

    public static bool isGameOver = false;

    private int lives;

    void Awake()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        lives = maxLives;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateHearts();
    }

    public void LoseLife()
    {
        if (isGameOver) return;

        lives--;
        UpdateHearts();
        Debug.Log("Lives left: " + lives);

        if (lives <= 0)
            GameOver();
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            bool alive = i < lives;

            if (fullHeart != null && emptyHeart != null)
            {
                hearts[i].enabled = true;
                hearts[i].sprite = alive ? fullHeart : emptyHeart;
            }
            else
            {
                hearts[i].enabled = alive;
            }
        }
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over!");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f; // freeze the game
    }

    // Hook this up to a Restart button's OnClick
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}