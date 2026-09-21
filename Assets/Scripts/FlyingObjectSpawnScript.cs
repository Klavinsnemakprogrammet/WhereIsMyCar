using UnityEngine;

public class FlyingObjectSpawnScript : MonoBehaviour

    
{
    ScreenBoundariesScript screenBoundariesScript;
    [Header("Cloud Prefabs")]
    public GameObject[] cloudPrefabs;
    [Header("Plane Prefabs")]
    public GameObject[] planePrefabs;

    [Header("Spawn Point")]
    public Transform spawnPoint;
    [Header("Spawn interval")]
    public float cloudSpawnInterval = 2f;
    public float planeSpawnInterval = 3f;

    private float minY, maxY;

    [Header("Cloud Speed")]
    public float CloudMinSpeed = 2f;
    public float CloudMaxSpeed = 100f;

    [Header("Plane Speed")]
    public float PlaneMinSpeed = 2f;
    public float PlaneMaxSpeed = 100f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        screenBoundariesScript = Object.FindFirstObjectByType<ScreenBoundariesScript>();
        minY = screenBoundariesScript.minY;
        maxY = screenBoundariesScript.maxY;
        InvokeRepeating(nameof(SpawnCloud), 0f, cloudSpawnInterval);
        InvokeRepeating(nameof(SpawnPlane), 0f, planeSpawnInterval);
    }



    // Update is called once per frame
    void SpawnCloud()
    {
        if (cloudPrefabs.Length == 0) return;
        GameObject prefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];
        float y = Random.Range(minY, maxY);
        Vector3 pos = new Vector3(spawnPoint.position.x, y, spawnPoint.position.z);
        GameObject cloud = Instantiate(prefab,pos, Quaternion.identity, spawnPoint);
        float speed = Random.Range(CloudMinSpeed, CloudMaxSpeed);

        FlyingObjectControllerScript controller = 
            cloud.GetComponent<FlyingObjectControllerScript>();
        if (controller != null)
            controller.speed = speed;
    }
    void SpawnPlane()
    {
        if (planePrefabs.Length == 0) return;
        GameObject prefab = planePrefabs[Random.Range(0, planePrefabs.Length)];
        float y = Random.Range(minY, maxY);
        Vector3 pos = new Vector3(-spawnPoint.position.x, y, spawnPoint.position.z);
        GameObject plane = Instantiate(prefab, pos, Quaternion.identity, spawnPoint);
        float speed = Random.Range(PlaneMinSpeed, PlaneMaxSpeed);

        FlyingObjectControllerScript controller =
            plane.GetComponent<FlyingObjectControllerScript>();
        if (controller != null)
            controller.speed = -speed;
    }
}

