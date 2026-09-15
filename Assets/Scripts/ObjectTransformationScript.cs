using UnityEngine;

public class ObjectTransformationScript : MonoBehaviour
{
    public GameObjectsScript gameObjectsScript;

    private void Awake()
    {
        gameObjectsScript = FindAnyObjectByType<GameObjectsScript>();
    }

    void Update()
    {
        if (GameObjectsScript.lastDragged != null)
        {
            if (Input.GetKey(KeyCode.Z))
            {
                GameObjectsScript.lastDragged.GetComponent<RectTransform>().Rotate(
                    0, 0, Time.deltaTime * 10);

            }
            if (Input.GetKey(KeyCode.X))
            {
                GameObjectsScript.lastDragged.GetComponent<RectTransform>().Rotate(
                    0, 0, -Time.deltaTime * 10);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                if (GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y < 1f)
                {
                    GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale =
                        new Vector3(GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.x,
                        GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y + 0.001f, 1f);

                }


            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                if (GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y > 0.3f)
                {
                    GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale =
                        new Vector3(GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.x,
                        GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y - 0.001f, 1f);

                }
            }
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    if (GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.x > 0.3f)
                    {
                        GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale =
                            new Vector3(GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.x - 0.001f,
                            GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y, 1f);

                    }
                }
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    if (GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.x < 0.9f)
                    {
                        GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale =
                            new Vector3(GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.x + 0.001f,
                            GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y, 1f);
                    
                    }
                }
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    if(GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.x < 0.9f)
                    {
                    GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale =
                    new Vector3(GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.x * -1f,
                    GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y, 1f);
                    }
                }
            }
        }
    }


