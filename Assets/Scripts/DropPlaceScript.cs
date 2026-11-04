using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    public GameObjectsScript gameObjectsScript;
    public GameFinishScript gameFinishScript;

    public float sizeTolerance = 0.15f; // max allowed size difference (was 0.08)

    void Awake()
    {
        gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();
        gameFinishScript = Object.FindFirstObjectByType<GameFinishScript>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        if (gameObjectsScript == null)
        {
            Debug.LogError("Cannot handle car placement because GameObjectsScript was not found.");
            return;
        }

        RectTransform carTransform = eventData.pointerDrag.GetComponent<RectTransform>();
        RectTransform placeTransform = GetComponent<RectTransform>();
        if (carTransform == null || placeTransform == null)
        {
            Debug.LogError("Cannot validate car placement because a RectTransform is missing.");
            return;
        }

        float rotationDifference = Mathf.Abs(Mathf.DeltaAngle(
            carTransform.eulerAngles.z, placeTransform.eulerAngles.z));
        Vector3 carSize = carTransform.localScale;
        Vector3 placeSize = placeTransform.localScale;
        float xSizeDifference = Mathf.Abs(Mathf.Abs(carSize.x) - Mathf.Abs(placeSize.x));
        float ySizeDifference = Mathf.Abs(carSize.y - placeSize.y);

        bool isCorrectPlacement = eventData.pointerDrag.tag == tag &&
            rotationDifference <= 7f &&
            xSizeDifference <= sizeTolerance &&
            ySizeDifference <= sizeTolerance;

        if (!isCorrectPlacement)
        {
            gameObjectsScript.inRightPlace = false;
            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[4]);

            if (!gameObjectsScript.ReturnCarToSpawn(eventData.pointerDrag))
                Debug.LogError($"No spawn position is registered for {eventData.pointerDrag.name}.");
            return;
        }

        Debug.Log("Car placed correctly!");
        gameObjectsScript.inRightPlace = true;

        carTransform.anchoredPosition = placeTransform.anchoredPosition;
        carTransform.localScale = placeTransform.localScale;
        carTransform.localRotation = placeTransform.localRotation;

        if (gameFinishScript != null)
            gameFinishScript.CarPlaced();

        PlayPlacementSound(eventData.pointerDrag.tag);
    }

    void PlayPlacementSound(string carTag)
    {
        int soundIndex;
        switch (carTag)
        {
            case "Garbage": soundIndex = 1; break;
            case "medicine": soundIndex = 2; break;
            case "School": soundIndex = 3; break;
            case "b2": soundIndex = 5; break;
            case "Police": soundIndex = 6; break;
            case "cement": soundIndex = 7; break;
            case "escavator": soundIndex = 8; break;
            case "e46": soundIndex = 9; break;
            case "e61": soundIndex = 10; break;
            case "tractor1": soundIndex = 11; break;
            case "tractor5": soundIndex = 12; break;
            case "firefighter": soundIndex = 13; break;
            default:
                Debug.LogWarning($"No placement sound is configured for car tag '{carTag}'.");
                return;
        }

        gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[soundIndex]);
    }
}