using System.Collections.Generic;
using UnityEngine;

public class GameObjectsScript : MonoBehaviour
{
    public static GameObjectsScript Instance;
    public static bool dragCancelled = false; // true after a car was sent back mid-drag

    private class SpawnState { public Vector3 pos, scale; public Quaternion rot; }
    private readonly Dictionary<GameObject, SpawnState> spawnStates = new Dictionary<GameObject, SpawnState>();
    [Header("Cars")]
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

    [Header("Places (same order as cars)")]
    public GameObject garbageTruckPlace;
    public GameObject medicinePlace;
    public GameObject schoolBusPlace;
    public GameObject policePlace;
    public GameObject b2Place;
    public GameObject cementPlace;
    public GameObject escavatorPlace;
    public GameObject e46Place;
    public GameObject e61Place;
    public GameObject tractor1Place;
    public GameObject tractor5Place;
    public GameObject firefighterPlace;

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

    [Range(0f, 1f)]
    public float flipChance = 0.5f; // chance that each object is mirrored horizontally (x * -1)

    public Canvas canvas;
    public AudioSource carSoundSource;
    public AudioClip[] sounds;

    [HideInInspector] public bool inRightPlace = false;
    public static GameObject lastDragged = null;
    public static bool isDragging = false;

    void Awake()
    {
        Instance = this;
        dragCancelled = false;
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

        // 3. Cars and their matching Places, in the same order
        GameObject[] cars =
        {
            garbageTruck, medicine, schoolBus, police, b2, cement,
            escavator, e46, e61, tractor1, tractor5, firefighter
        };

        GameObject[] places =
        {
            garbageTruckPlace, medicinePlace, schoolBusPlace, policePlace, b2Place, cementPlace,
            escavatorPlace, e46Place, e61Place, tractor1Place, tractor5Place, firefighterPlace
        };

        if (slots.Count < cars.Length + places.Length)
        {
            Debug.LogError($"Need at least {cars.Length + places.Length} slots, but only found {slots.Count}.");
            return;
        }

        // 4. Cars take shuffled slots 0..11
        for (int i = 0; i < cars.Length; i++)
        {
            RectTransform rt = cars[i].GetComponent<RectTransform>();
            rt.localPosition = slots[i].GetComponent<RectTransform>().localPosition;
            FlipRandomly(rt);
        }

        // 5. Places take shuffled slots 12..23 (no overlap with cars)
        for (int i = 0; i < places.Length; i++)
        {
            RectTransform rt = places[i].GetComponent<RectTransform>();
            rt.localPosition = slots[cars.Length + i].GetComponent<RectTransform>().localPosition;
            FlipRandomly(rt);
        }

        // 6. Cache the new "correct" coords for each car
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
        
        spawnStates.Clear();
        foreach (GameObject car in cars)
        {
            RectTransform r = car.GetComponent<RectTransform>();
            spawnStates[car] = new SpawnState { pos = r.localPosition, scale = r.localScale, rot = r.localRotation };
        }
    }

    void FlipRandomly(RectTransform rt)
    {
        if (Random.value < flipChance)
        {
            Vector3 scale = rt.localScale;
            scale.x *= -1f;
            rt.localScale = scale;
        }
    }
    public void ReturnToSpawn(GameObject car)
    {
        if (!spawnStates.TryGetValue(car, out SpawnState s)) return;

        RectTransform rt = car.GetComponent<RectTransform>();
        rt.localPosition = s.pos;
        rt.localScale = s.scale;
        rt.localRotation = s.rot;

        // Cancel the current drag so OnDrag stops moving the car
        dragCancelled = true;
        isDragging = false;
        lastDragged = null;
    }
}