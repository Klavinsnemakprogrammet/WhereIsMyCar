using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class FlyingObjectControllerScript : MonoBehaviour
{
    [HideInInspector]
    public float speed = 1f;
    public float fadeDuration = 1.5f;
    public float waveAmplitude = 25f;
    public float waveFrequency = 1f;
    private GameObjectsScript gameObjectsScript;
    private ScreenBoundariesScript screenBoundariesScript;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private bool isFadingOut = false;
    private bool isExploading = false;
    private Image image;
    private Color originalColor;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if(canvasGroup == null ) 
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        if( image != null )
            originalColor = image.color;

        gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();
        screenBoundariesScript = Object.FindFirstObjectByType<ScreenBoundariesScript>();
        StartCoroutine(FadeIn());
    }
    IEnumerator FadeIn()
    {
        float time = 0f;
        while ( time < fadeDuration ) 
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
    private void Update()
    {
        float waveOffset = Mathf.Sin( Time.time * waveFrequency) * waveAmplitude;
        rectTransform.anchoredPosition += new Vector2(-speed * Time.deltaTime, waveOffset * Time.deltaTime);
        
        if(speed > 0 && transform.position.x < (screenBoundariesScript.minX + 80) && !isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
            isFadingOut = true;
        }

        if (speed < 0 && transform.position.x > (screenBoundariesScript.maxX - 80) && !isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
            isFadingOut = true;
        }
        // velak pieliksim sadursmi ar bumbu un makoniem 

    }

    IEnumerator FadeOutAndDestroy()
    {
        float time = 0f;
        float startAlpha = canvasGroup.alpha;
        while ( time < fadeDuration ) 
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, time / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        Destroy(gameObject);
    }
}
