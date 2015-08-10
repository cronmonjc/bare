using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Component.  Toggle to allow user to disable default wiring scheme for screwy wiring schemes.
/// </summary>
public class OverrideWiring : MonoBehaviour {
    /// <summary>
    /// The Text component.  Set via Unity Inspector.
    /// </summary>
    public Text t;
    /// <summary>
    /// The Button component.
    /// </summary>
    private Button b;

    /// <summary>
    /// ColorBlock to use when we are overriding.  Set via Unity Inspector.
    /// </summary>
    public ColorBlock tru = new ColorBlock() { colorMultiplier = 1f, normalColor = Color.green, highlightedColor = new Color(0.25f, 0.9f, 0.25f), pressedColor = new Color(0f, 0.75f, 0f), disabledColor = new Color32(200, 200, 200, 128) };
    /// <summary>
    /// ColorBlock to use when we are not overriding.  Set via Unity Inspector.
    /// </summary>
    public ColorBlock fal = new ColorBlock() { colorMultiplier = 1f, normalColor = Color.red, highlightedColor = new Color(0.9f, 0.25f, 0.25f), pressedColor = new Color(0.75f, 0f, 0f), disabledColor = new Color32(200, 200, 200, 128) };

    /// <summary>
    /// Called when the user clicks the button
    /// </summary>
    public void Clicked() {
        LightLabel.wireOverride = !LightLabel.wireOverride;

        t.text = (LightLabel.wireOverride ? "YES" : "NO");

        b.colors = (LightLabel.wireOverride ? tru : fal);
    }

    /// <summary>
    /// Called immediately when the Component's GameObject is enabled
    /// </summary>
    void OnEnable() {
        if(b == null) b = GetComponent<Button>();

        t.text = (LightLabel.wireOverride ? "YES" : "NO");

        b.colors = (LightLabel.wireOverride ? tru : fal);
    }
}
