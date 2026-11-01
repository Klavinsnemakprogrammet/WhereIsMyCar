using UnityEngine;

public class HorizontalLock : MonoBehaviour
{
    private ScreenOrientation previousOrientation;

    private void Awake()
    {
        previousOrientation = Screen.orientation;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    private void OnDestroy()
    {
        Screen.orientation = previousOrientation;
    }
}
