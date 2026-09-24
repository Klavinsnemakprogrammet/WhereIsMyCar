using TMPro;
using UnityEngine;

public class TimerScript : MonoBehaviour
{
    [Tooltip("The TMP text that shows the time. If empty, the first TMP text in this object's children is used.")]
    public TMP_Text timerText;

    [Tooltip("Optional - the timer freezes when the victory screen shows up")]
    public GameFinishScript gameFinishScript;

    private float elapsed = 0f;

    void Awake()
    {
        if (timerText == null)
            timerText = GetComponentInChildren<TMP_Text>();

        if (gameFinishScript == null)
            gameFinishScript = Object.FindFirstObjectByType<GameFinishScript>();
    }

    void Start()
    {
        UpdateText();
    }

    void Update()
    {
        // Stop counting once the game is won
        if (gameFinishScript != null && gameFinishScript.IsFinished)
            return;

        elapsed += Time.deltaTime;
        UpdateText();
    }

    void UpdateText()
    {
        if (timerText != null)
            timerText.text = FormatTime(elapsed);
    }

    string FormatTime(float seconds)
    {
        int total = Mathf.FloorToInt(seconds);
        int h = total / 3600;
        int m = (total % 3600) / 60;
        int s = total % 60;
        return string.Format("{0:00}:{1:00}:{2:00}", h, m, s);
    }
}