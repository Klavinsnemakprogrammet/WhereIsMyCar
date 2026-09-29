using UnityEngine;

public class CarHitDetectorScript : MonoBehaviour
{
    public LivesScript livesScript;

    [Header("Settings")]
    public float hitCooldown = 1.5f;      // seconds before another hit can count
    [Range(0.3f, 1f)]
    public float hitboxScale = 0.8f;      // <1 = more forgiving, only real overlaps count

    private float lastHitTime = -999f;
    private readonly Vector3[] corners = new Vector3[4];

    void Awake()
    {
        if (livesScript == null)
            livesScript = Object.FindFirstObjectByType<LivesScript>();
    }

    void Update()
    {
        if (LivesScript.isGameOver) return;

        // Only the car currently being dragged can hit anything
        if (!GameObjectsScript.isDragging || GameObjectsScript.lastDragged == null) return;

        if (Time.time - lastHitTime < hitCooldown) return;

        GameObject car = GameObjectsScript.lastDragged;
        Rect carRect = GetWorldRect(car.GetComponent<RectTransform>());

        foreach (HazardScript hazard in HazardScript.All)
        {
            if (!hazard.gameObject.activeInHierarchy) continue;

            Rect hazardRect = GetWorldRect(hazard.GetComponent<RectTransform>());

            if (carRect.Overlaps(hazardRect))
            {
                lastHitTime = Time.time;
                Debug.Log(car.name + " hit " + hazard.name + "!");
                livesScript.LoseLife();
                break;
            }
        }
    }

    // Bounding box in world space, works for rotated and mirrored objects
    Rect GetWorldRect(RectTransform rt)
    {
        rt.GetWorldCorners(corners);

        float minX = corners[0].x, maxX = corners[0].x;
        float minY = corners[0].y, maxY = corners[0].y;

        for (int i = 1; i < 4; i++)
        {
            minX = Mathf.Min(minX, corners[i].x);
            maxX = Mathf.Max(maxX, corners[i].x);
            minY = Mathf.Min(minY, corners[i].y);
            maxY = Mathf.Max(maxY, corners[i].y);
        }

        Vector2 center = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
        Vector2 size = new Vector2(maxX - minX, maxY - minY) * hitboxScale;
        return new Rect(center - size * 0.5f, size);
    }
}