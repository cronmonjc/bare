using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/// <summary>
/// UI Component.  Debugging.  Listed off everything the StandaloneInputModule was seeing.
/// </summary>
public class EventSysDebug : MonoBehaviour {
    /// <summary>
    /// The PointerInputModule Component it was using
    /// </summary>
    private PointerInputModule pim;
    /// <summary>
    /// The Text Component reference
    /// </summary>
    private Text t;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
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
