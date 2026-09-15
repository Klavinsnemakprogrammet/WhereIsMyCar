using UnityEngine;

public class GameObjectsScript : MonoBehaviour
{
    public GameObject garbageTruck;
    public GameObject medicine;
    public GameObject schoolBus;
    public GameObject police;
    public GameObject b2;
    public GameObject cement;
    public GameObject escavator;
    public GameObject e46;
    public GameObject e61;
    public GameObject tractor1;
    public GameObject tractor5;
    public GameObject firefighter;

    [HideInInspector]
    public Vector2 garbageTruckCoord;
    [HideInInspector]
    public Vector2 medicineCoord;
    [HideInInspector]
    public Vector2 schoolBusCoord;
    [HideInInspector]
    public Vector2 policeCoord;
    [HideInInspector]
    public Vector2 b2Coord;
    [HideInInspector]
    public Vector2 cementCoord;
    [HideInInspector]
    public Vector2 escavatorCoord;
    [HideInInspector]
    public Vector2 e46Coord;
    [HideInInspector]
    public Vector2 e61Coord;
    [HideInInspector]
    public Vector2 tractor1Coord;
    [HideInInspector]
    public Vector2 tractor5Coord;
    [HideInInspector]
    public Vector2 firefighterCoord;

    public Canvas canvas;
    public AudioSource carSoundSource;
    public AudioClip[] sounds;

    [HideInInspector]
    public bool inRightPlace = false;
    public static GameObject lastDragged = null;
    public static bool isDragging = false;



    void Awake()
    {
        garbageTruckCoord = garbageTruck.GetComponent<RectTransform>().localPosition;
        medicineCoord = medicine.GetComponent<RectTransform>().localPosition;
        schoolBusCoord = schoolBus.GetComponent<RectTransform>().localPosition;
        policeCoord = police.GetComponent<RectTransform>().localPosition;
        b2Coord = b2.GetComponent<RectTransform>().localPosition;
        cementCoord = cement.GetComponent<RectTransform>().localPosition;
        escavatorCoord = escavator.GetComponent<RectTransform>().localPosition;
        e46Coord = e46.GetComponent<RectTransform>().localPosition;
        e61Coord = e61.GetComponent<RectTransform>().localPosition;
        tractor1Coord = tractor1.GetComponent<RectTransform>().localPosition;
        tractor5Coord = tractor5.GetComponent<RectTransform>().localPosition;
        firefighterCoord = firefighter.GetComponent<RectTransform>().localPosition;
    }
}