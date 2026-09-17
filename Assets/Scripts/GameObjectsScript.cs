using System.Collections.Generic;
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

    public Transform carPlaces; // parent holding the 32 empties named "1".."32"

    [HideInInspector] public Vector2 garbageTruckCoord;
    [HideInInspector] public Vector2 medicineCoord;
    [HideInInspector] public Vector2 schoolBusCoord;
    [HideInInspector] public Vector2 policeCoord;
    [HideInInspector] public Vector2 b2Coord;
    [HideInInspector] public Vector2 cementCoord;
    [HideInInspector] public Vector2 escavatorCoord;
    [HideInInspector] public Vector2 e46Coord;
    [HideInInspector] public Vector2 e61Coord;
    [HideInInspector] public Vector2 tractor1Coord;
    [HideInInspector] public Vector2 tractor5Coord;
    [HideInInspector] public Vector2 firefighterCoord;

    public Canvas canvas;
    public AudioSource carSoundSource;
    public AudioClip[] sounds;

    [HideInInspector] public bool inRightPlace = false;
    public static GameObject lastDragged = null;
    public static bool isDragging = false;

    void Awake()
    {
        AssignRandomPositions();
    }

    void AssignRandomPositions()
    {
        // 1. Collect all 32 empties
        List<Transform> slots = new List<Transform>();
        foreach (Transform child in carPlaces)
            slots.Add(child);

        // 2. Fisher-Yates shuffle
        for (int i = slots.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (slots[i], slots[j]) = (slots[j], slots[i]);
        }

        // 3. Cars in the same order you want them placed
        GameObject[] cars =
        {
            garbageTruck, medicine, schoolBus, police, b2, cement,
            escavator, e46, e61, tractor1, tractor5, firefighter
        };

        // 4. Assign the first 12 shuffled slots to the cars
        for (int i = 0; i < cars.Length; i++)
        {
            Vector2 pos = slots[i].GetComponent<RectTransform>().localPosition;
            cars[i].GetComponent<RectTransform>().localPosition = pos;
        }

        // 5. Cache the new "correct" coords for each car
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