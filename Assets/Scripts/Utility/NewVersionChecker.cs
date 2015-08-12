using UnityEngine;
using System.Collections;
using System.IO;

/// <summary>
/// UI Component.  Changes the Revision text when it finds a certain file.
/// </summary>
public class NewVersionChecker : MonoBehaviour {
    /// <summary>
    /// The path to the file it's looking for
    /// </summary>
    public string path;
    /// <summary>
    /// Whether or not this Component has found the file
    /// </summary>
    [System.NonSerialized]
    public bool fired = false;

    /// <summary>
    /// The reference to the Text UI Component
    /// </summary>
    public UnityEngine.UI.Text text;

    /// <summary>
    /// How much longer until it checks again
    /// </summary>
    public float countdown = 0.0f;
    /// <summary>
    /// How much time should pass between checks
    /// </summary>
    public float loopTime = 10.0f;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
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
                    fired = true;
                }
            }
        }
    }
}
