using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DebugMousePos : MonoBehaviour {
    private Text t;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(t == null) t = GetComponent<Text>();

        t.text = Input.mousePosition.ToString("F3");
    }
}
