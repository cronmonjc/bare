using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DebugMousePos : MonoBehaviour {
    private Text t;

    // Update is called once per frame
    void Update() {
        if(t == null) t = GetComponent<Text>();

        t.text = Input.mousePosition.ToString("F3");
    }
}
