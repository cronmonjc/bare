using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AltNumbering : MonoBehaviour {
    public Text t;
    private Button b;

    public void Clicked() {
        LightLabel.alternateNumbering = !LightLabel.alternateNumbering;
    }

    void Update() {
        if(b == null) b = GetComponent<Button>();

        t.text = (LightLabel.alternateNumbering ? "From Corners In" : "Around the Bar");

        ColorBlock cb = b.colors;

        cb.normalColor = (LightLabel.alternateNumbering ? new Color32(255, 216, 0, 255) : new Color32(0, 255, 255, 255));
        cb.highlightedColor = (LightLabel.alternateNumbering ? new Color32(255, 223, 63, 255) : new Color32(63, 255, 255, 255));
        cb.pressedColor = (LightLabel.alternateNumbering ? new Color32(191, 159, 0, 255) : new Color32(0, 191, 191, 255));

        b.colors = cb;
    }
}
