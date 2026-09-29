using UnityEngine;

public class HazardHitScript : MonoBehaviour
{
    [Range(0.2f, 1f)]
    public float carHitboxScale = 0.7f; // shrinks the car's rectangle so transparent edges don't count

    private Collider2D hazardCol;

    void Awake()
    {
        hazardCol = GetComponent<Collider2D>();
        if (hazardCol == null)
            Debug.LogWarning(name + " has no Collider2D, so it can't hit anything!", this);
    }

    void Update()
    {
        if (hazardCol == null || !hazardCol.enabled) return;
        if (LivesScript.isGameOver || LivesScript.Instance == null) return;

        // Only the car currently being held counts
        GameObject car = GameObjectsScript.lastDragged;
        if (!GameObjectsScript.isDragging || !Input.GetMouseButton(0) || car == null) return;

        if (IsTouchingCar(car))
        {
            Debug.Log(car.name + " hit " + name + "!");

            bool lostLife = LivesScript.Instance.LoseLife();

            // Send the car home only if a life was lost and the player is still alive
            if (lostLife && !LivesScript.isGameOver && GameObjectsScript.Instance != null)
                GameObjectsScript.Instance.ReturnToSpawn(car);
        }
    }

    bool IsTouchingCar(GameObject car)
    {
        RectTransform rt = car.GetComponent<RectTransform>();

        // The car's rectangle in world space (handles rotation, scale, flip and pivot)
        Vector2 center = rt.TransformPoint(rt.rect.center);
        Vector3 s = rt.lossyScale;
        Vector2 size = new Vector2(rt.rect.width * Mathf.Abs(s.x),
                                   rt.rect.height * Mathf.Abs(s.y)) * carHitboxScale;
        float angle = rt.eulerAngles.z;

        Physics2D.SyncTransforms(); // the car was just moved by dragging

        Collider2D[] found = Physics2D.OverlapBoxAll(center, size, angle);
        foreach (Collider2D c in found)
        {
            if (c == hazardCol) return true;
        }
        return false;
    }
}