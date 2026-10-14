using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectTransformationScript : MonoBehaviour
{
    public float rotationSpeed = 90f;
    public float scaleSpeed = 0.5f;
    public float minScale = 0.3f;
    public float maxScale = 0.9f;

    private bool rotateCW, rotateCCW;
    private bool scaleUpX, scaleDownX, scaleUpY, scaleDownY;

    public static bool isTransforming = false;

    void Update()
    {
        if (GameObjectsScript.lastDragged == null)
        {
            isTransforming = false;
            return;
        }

        RectTransform rc = GameObjectsScript.lastDragged.GetComponent<RectTransform>();
        if (rc == null)
        {
            isTransforming = false;
            return;
        }

        float rotateStep = rotationSpeed * Time.deltaTime;
        float scaleStep = scaleSpeed * Time.deltaTime;

        if (rotateCW) rc.Rotate(0, 0, -rotateStep);
        if (rotateCCW) rc.Rotate(0, 0, rotateStep);

        if (scaleUpY) ChangeScale(rc, false, scaleStep);
        if (scaleDownY) ChangeScale(rc, false, -scaleStep);
        if (scaleUpX) ChangeScale(rc, true, scaleStep);
        if (scaleDownX) ChangeScale(rc, true, -scaleStep);

        isTransforming = rotateCW || rotateCCW || scaleUpY || scaleDownY || scaleUpX || scaleDownX;
    }

    // Changes the size on one axis while keeping the flip (negative scale) intact.
    private void ChangeScale(RectTransform rc, bool xAxis, float delta)
    {
        Vector3 s = rc.localScale;
        float current = xAxis ? s.x : s.y;
        float sign = current < 0f ? -1f : 1f;
        float newSize = Mathf.Clamp(Mathf.Abs(current) + delta, minScale, maxScale);

        if (xAxis) s.x = newSize * sign;
        else s.y = newSize * sign;

        rc.localScale = s;
    }

    // ---- Flip buttons (hook these up to a UI Button's OnClick) ----

    // Mirrors the object left <-> right
    public void FlipHorizontal()
    {
        FlipAxis(true);
    }

    // Mirrors the object top <-> bottom
    public void FlipVertical()
    {
        FlipAxis(false);
    }

    private void FlipAxis(bool xAxis)
    {
        if (GameObjectsScript.lastDragged == null) return;

        RectTransform rc = GameObjectsScript.lastDragged.GetComponent<RectTransform>();
        if (rc == null) return;

        Vector3 s = rc.localScale;
        if (xAxis) s.x = -s.x;
        else s.y = -s.y;
        rc.localScale = s;
    }

    // ---- Press-and-hold handlers (for EventTrigger PointerDown / PointerUp) ----
    public void StartRotateCW(BaseEventData eventData) { rotateCW = true; }
    public void StopRotateCW(BaseEventData eventData) { rotateCW = false; }
    public void StartRotateCCW(BaseEventData eventData) { rotateCCW = true; }
    public void StopRotateCCW(BaseEventData eventData) { rotateCCW = false; }

    public void StartScaleUpY(BaseEventData eventData) { scaleUpY = true; }
    public void StopScaleUpY(BaseEventData eventData) { scaleUpY = false; }
    public void StartScaleDownY(BaseEventData eventData) { scaleDownY = true; }
    public void StopScaleDownY(BaseEventData eventData) { scaleDownY = false; }

    public void StartScaleUpX(BaseEventData eventData) { scaleUpX = true; }
    public void StopScaleUpX(BaseEventData eventData) { scaleUpX = false; }
    public void StartScaleDownX(BaseEventData eventData) { scaleDownX = true; }
    public void StopScaleDownX(BaseEventData eventData) { scaleDownX = false; }
}