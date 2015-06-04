using UnityEngine;
using UnityEngine.UI;

public class OverrideWiring : MonoBehaviour {
    public Text t;
    private Button b;

    public ColorBlock tru = new ColorBlock() { colorMultiplier = 1f, normalColor = Color.green, highlightedColor = new Color(0.25f, 0.9f, 0.25f), pressedColor = new Color(0f, 0.75f, 0f), disabledColor = new Color32(200, 200, 200, 128) },
                      fal = new ColorBlock() { colorMultiplier = 1f, normalColor = Color.red, highlightedColor = new Color(0.9f, 0.25f, 0.25f), pressedColor = new Color(0.75f, 0f, 0f), disabledColor = new Color32(200, 200, 200, 128) };

    public void Clicked() {
        LightLabel.wireOverride = !LightLabel.wireOverride;

        t.text = (LightLabel.wireOverride ? "YES" : "NO");

        b.colors = (LightLabel.wireOverride ? tru : fal);
    }

    void OnEnable() {
        if(b == null) b = GetComponent<Button>();

        t.text = (LightLabel.wireOverride ? "YES" : "NO");

        b.colors = (LightLabel.wireOverride ? tru : fal);
    }
}
