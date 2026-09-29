using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ClickDebugScript : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        var data = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        if (results.Count == 0) Debug.Log("Click hit nothing");
        for (int i = 0; i < results.Count; i++)
            Debug.Log(i + ": " + results[i].gameObject.name);
    }
}