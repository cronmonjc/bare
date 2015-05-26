using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShowScreenSize : MonoBehaviour {
    private Text t;

    void Update() {
        if(t == null) t = GetComponent<Text>();

        t.text = "Debug: " + Screen.width + "w*" + Screen.height + "h";
    }
}
