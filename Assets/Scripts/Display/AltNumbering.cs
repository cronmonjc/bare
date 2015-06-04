using UnityEngine;
using UnityEngine.UI;

public class AltNumbering : MonoBehaviour {
    public Text t;
    private Button b;

    public ColorBlock tru = new ColorBlock() { colorMultiplier = 1f, normalColor = new Color32(255, 216, 0, 255), highlightedColor = new Color32(255, 223, 63, 255), pressedColor = new Color32(191, 159, 0, 255), disabledColor = new Color32(200, 200, 200, 128) },
                      fal = new ColorBlock() { colorMultiplier = 1f, normalColor = new Color32(0, 255, 255, 255), highlightedColor = new Color32(63, 255, 255, 255), pressedColor = new Color32(0, 191, 191, 255), disabledColor = new Color32(200, 200, 200, 128) };

    public void Clicked() {
        LightLabel.alternateNumbering = !LightLabel.alternateNumbering;

        t.text = (LightLabel.alternateNumbering ? "From Corners In" : "Around the Bar");

        b.colors = (LightLabel.alternateNumbering ? tru : fal);
    }

    void OnEnable() {
        if(b == null) b = GetComponent<Button>();

        t.text = (LightLabel.alternateNumbering ? "From Corners In" : "Around the Bar");

        b.colors = (LightLabel.alternateNumbering ? tru : fal);
    }
}
