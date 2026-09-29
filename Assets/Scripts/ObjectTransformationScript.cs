using UnityEngine;

public class ObjectTransformationScript : MonoBehaviour
{
    public GameObjectsScript gameObjectsScript;

    [Header("Speeds")]
    public float rotateSpeed = 40f;   // degrees per second (was 10)
    public float scaleSpeed = 0.25f;  // scale units per second (was ~0.06 at 60 FPS)

    private const float MinScale = 0.3f;
    private const float MaxScale = 1f;

    private void Awake()
    {
        gameObjectsScript = FindAnyObjectByType<GameObjectsScript>();
    }

    void Update()
    {
        if (GameObjectsScript.lastDragged == null) return;

        RectTransform rt = GameObjectsScript.lastDragged.GetComponent<RectTransform>();

        // Rotate
        if (Input.GetKey(KeyCode.Z)) rt.Rotate(0, 0, rotateSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.X)) rt.Rotate(0, 0, -rotateSpeed * Time.deltaTime);

        // Scale (works for both normal and mirrored objects)
        Vector3 s = rt.localScale;
        float signX = s.x < 0 ? -1f : 1f;
        float absX = Mathf.Abs(s.x);
        float y = s.y;
        float step = scaleSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.UpArrow)) y += step;
        if (Input.GetKey(KeyCode.DownArrow)) y -= step;
        if (Input.GetKey(KeyCode.RightArrow)) absX += step;
        if (Input.GetKey(KeyCode.LeftArrow)) absX -= step;

        y = Mathf.Clamp(y, MinScale, MaxScale);
        absX = Mathf.Clamp(absX, MinScale, MaxScale);

        rt.localScale = new Vector3(signX * absX, y, 1f);

        // Flip horizontally
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 f = rt.localScale;
            f.x *= -1f;
            rt.localScale = f;
        }
    }
}