using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    private float placeZRot, carZRot, diffZRot;
    private Vector3 placeSize, carSize;
    private float xSizeDiff, ySizeDiff;
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
        if ((eventData.pointerDrag != null) && Input.GetMouseButtonUp(0) &&
            (!Input.GetMouseButton(2)))
        {
            if (eventData.pointerDrag.tag.Equals(tag))
            {
                placeZRot =
                    eventData.pointerDrag.GetComponent<RectTransform>().transform.eulerAngles.z;
                carZRot = GetComponent<RectTransform>().transform.eulerAngles.z;
                diffZRot = Mathf.Abs(placeZRot - carZRot);
                Debug.Log("Diff Z Rot: " + diffZRot);

                placeSize = eventData.pointerDrag.GetComponent<RectTransform>().localScale;
                carSize = GetComponent<RectTransform>().localScale;

                // Compare the size ignoring the flip direction (a flipped car has a negative x scale)
                xSizeDiff = Mathf.Abs(Mathf.Abs(placeSize.x) - Mathf.Abs(carSize.x));
                ySizeDiff = Mathf.Abs(placeSize.y - carSize.y);
                Debug.Log("Diff X Size: " + xSizeDiff);
                Debug.Log("Diff Y Size: " + ySizeDiff);

                if ((diffZRot <= 7 || (diffZRot >= 353 && diffZRot <= 360)) &&
                    (xSizeDiff <= sizeTolerance && ySizeDiff <= sizeTolerance))
                {
                    Debug.Log("Car placed correctly!");
                    gameObjectsScript.inRightPlace = true;

                    if (gameFinishScript != null)
                        gameFinishScript.CarPlaced();

                    eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition =
                        GetComponent<RectTransform>().anchoredPosition;

                    // Copies the place's scale, so a flipped car snaps to the place's orientation
                    eventData.pointerDrag.GetComponent<RectTransform>().localScale =
                        GetComponent<RectTransform>().localScale;

                    eventData.pointerDrag.GetComponent<RectTransform>().localRotation =
                        GetComponent<RectTransform>().localRotation;

                    switch (eventData.pointerDrag.tag)
                    {
                        case "Garbage":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[1]);
                            break;

                        case "medicine":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[2]);
                            break;

                        case "School":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[3]);
                            break;

                        case "b2":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[5]);
                            break;

                        case "Police":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[6]);
                            break;

                        case "cement":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[7]);
                            break;

                        case "escavator":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[8]);
                            break;

                        case "e46":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[9]);
                            break;

                        case "e61":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[10]);
                            break;

                        case "tractor1":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[11]);
                            break;

                        case "tractor5":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[12]);
                            break;

                        case "firefighter":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[13]);
                            break;

                        default:
                            Debug.Log("No matching tag found for the dropped object.");
                            break;
                    }
                }

            }
            else
            {
                gameObjectsScript.inRightPlace = false;
                gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[4]);

                switch (eventData.pointerDrag.tag)
                {
                    case "Garbage":
                        gameObjectsScript.garbageTruck.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.garbageTruckCoord;
                        break;

                    case "Ambulance":
                        gameObjectsScript.medicine.GetComponent<RectTransform>().localPosition =
                             gameObjectsScript.medicineCoord;
                        break;

                    case "School":
                        gameObjectsScript.schoolBus.GetComponent<RectTransform>().localPosition =
                             gameObjectsScript.schoolBusCoord;
                        break;

                    default:
                        Debug.Log("No matching tag found for the dropped object.");
                        break;
                }
            }
        }
    }
}