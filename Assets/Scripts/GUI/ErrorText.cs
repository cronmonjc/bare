using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// UI Component.  Displays errors and other information to the user.
/// </summary>
[RequireComponent(typeof(Text))]
public class ErrorText : MonoBehaviour {
    /// <summary>
    /// The single instance of this class
    /// </summary>
    public static ErrorText inst;
    /// <summary>
    /// The Coroutine controlling the fading of this Component
    /// </summary>
    private Coroutine fader;
    /// <summary>
    /// The Text Component reference
    /// </summary>
    private Text t;

    /// <summary>
    /// Awake is called once, immediately as the object is created (typically at load time)
    /// </summary>
    void Awake() {
        if(inst == null) inst = this;

        t = GetComponent<Text>();
        t.text = "";
    }

    /// <summary>
    /// Displays an error.
    /// </summary>
    /// <param name="msg">The message to show.</param>
    public void DispError(string msg) {
        t.color = new Color32(172, 0, 0, 255);
        if(fader != null) StopCoroutine(fader);
        fader = StartCoroutine(DispErrorThenFade(msg));
    }

    /// <summary>
    /// Displays information.
    /// </summary>
    /// <param name="msg">The message to show.</param>
    public void DispInfo(string msg) {
        t.color = new Color32(50, 50, 50, 255);
        if(fader != null) StopCoroutine(fader);
        fader = StartCoroutine(DispErrorThenFade(msg));
    }

    /// <summary>
    /// Coroutine.  Displays an error then fades.
    /// </summary>
    /// <param name="msg">The message to show.</param>
    public IEnumerator DispErrorThenFade(string msg) {
        Color32 c = t.color;
        c.a = 255;
        t.color = c;
        t.text = msg;

        yield return new WaitForSeconds(5f);

        for(; (c.a - 4) > 0f; c.a -= 4) {
            t.color = c;
            yield return new WaitForSeconds(0.1f);
        }

        t.text = "";
        c.a = 255;
        t.color = c;
        fader = null;
    }
    
}
