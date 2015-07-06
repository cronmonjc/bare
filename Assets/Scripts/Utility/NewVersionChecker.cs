using UnityEngine;
using System.Collections;
using System.IO;

public class NewVersionChecker : MonoBehaviour {
    public string path;
    [System.NonSerialized]
    public bool fired = false;

    public UnityEngine.UI.Text text;

    public float countdown = 0.0f, loopTime = 10.0f;

    void Update() {
        if(text == null) text = GetComponent<UnityEngine.UI.Text>();

        if(!fired) {
            if(countdown > 0.0f) {
                countdown -= Time.deltaTime;
            } else {
                countdown = loopTime;
                if(File.Exists(path)) {
                    text.text = "A new build is ready!\n" + File.ReadAllText(path) + "\nPlease close this application at your earliest convenience.";
                    text.color = Color.red;
                }
            }
        }
    }
}
