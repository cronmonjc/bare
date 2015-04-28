using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ErrorText : MonoBehaviour {
    public static ErrorText inst;
    private Text t;

    void Awake() {
        if(inst == null) inst = this;

        t = GetComponent<Text>();
        t.text = "";
    }

    public void DispError(string msg) {
        t.text = msg;
    }
    
}
