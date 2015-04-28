using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChangeColors : MonoBehaviour {

    public Color CurrentNormal, CurrentHighlight, IdleNormal, IdleHighlight;

    public void Switch(bool curr) {
        Button b = GetComponent<Button>();
        ColorBlock cb = b.colors;
        if(curr) {
            cb.normalColor = CurrentNormal;
            cb.highlightedColor = CurrentHighlight;
        } else {
            cb.normalColor = IdleNormal;
            cb.highlightedColor = IdleHighlight;
        }
        b.colors = cb;
    }

}
