using UnityEngine;
using UnityEngine.UI;

public class OverrideWiring : MonoBehaviour {
    public Text t;
    private Button b;

    public void Clicked() {
        LightLabel.wireOverride = !LightLabel.wireOverride;
    }

    void Update() {
        if(b == null) b = GetComponent<Button>();

        t.text = (LightLabel.wireOverride ? "YES" : "NO");

        ColorBlock cb = b.colors;

        cb.normalColor = (LightLabel.wireOverride ? Color.green : Color.red);
        cb.highlightedColor = (LightLabel.wireOverride ? new Color(0.25f, 0.9f, 0.25f) : new Color(0.9f, 0.25f, 0.25f));
        cb.pressedColor = (LightLabel.wireOverride ? new Color(0f, 0.75f, 0f) : new Color(0.75f, 0f, 0f));

        b.colors = cb;
    }
}
