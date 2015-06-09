using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class EventSysDebug : MonoBehaviour {
    private PointerInputModule pim;
    private Text t;

    void Update() {
        if(pim == null) pim = GetComponent<StandaloneInputModule>();
        if(t == null) t = GetComponent<Text>();

        PointerEventData ped = new PointerEventData(EventSystem.current);
        ped.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        if(results.Count > 0) {
            Debug.Log("Begin");

            foreach(RaycastResult alpha in results) {
                Debug.Log(alpha.gameObject.transform.GetPath());
            }

            Debug.Log("End");
        }


        //string text = "MouseOver: ";
        

    }
}
