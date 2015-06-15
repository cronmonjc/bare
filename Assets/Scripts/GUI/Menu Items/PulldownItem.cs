using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public abstract class PulldownItem : MonoBehaviour {
    public int number;
    public ColorBlock selected = new ColorBlock() { colorMultiplier = 1f, normalColor = new Color32(255, 192, 0, 255), highlightedColor = new Color32(255, 223, 127, 255), pressedColor = new Color32(255, 192, 0, 255), disabledColor = new Color32(255, 144, 144, 255) },
                    unselected = new ColorBlock() { colorMultiplier = 1f, normalColor = new Color32(255, 255, 255, 255), highlightedColor = new Color32(245, 245, 245, 255), pressedColor = new Color32(200, 200, 200, 255), disabledColor = new Color32(255, 144, 144, 255) };
    protected Button b;
    private bool prev = false;

    protected abstract bool IsSelected();
    public abstract void Clicked();

    void Update() {
        if(b == null) b = GetComponent<Button>();
        bool curr = IsSelected();
        if(prev ^ curr) {
            b.colors = curr ? selected : unselected;
            prev = curr;
        }
        
    }
}
