using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ErrorText : MonoBehaviour {
    public static ErrorText inst;
    private Coroutine fader;
    private Text t;

    void Awake() {
        if(inst == null) inst = this;

        t = GetComponent<Text>();
        t.text = "";
    }

    public void DispError(string msg) {
        if(fader != null) StopCoroutine(fader);
        fader = StartCoroutine(DispErrorThenFade(msg));
    }

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
