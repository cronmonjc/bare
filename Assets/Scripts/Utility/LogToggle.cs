using UnityEngine;
using System.Collections;

public class LogToggle : MonoBehaviour {
    public UnityEngine.UI.Text t;

    public void Clicked() {
        ErrorLogging.logInput = !ErrorLogging.logInput;

        t.text = (ErrorLogging.logInput ? "" : "Not") + " Logging Input";
    }
}
