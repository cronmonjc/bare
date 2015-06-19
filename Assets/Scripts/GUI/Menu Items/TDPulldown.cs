using UnityEngine;
using System.Collections;

public class TDPulldown : PulldownItem {
    public Color textEnabled = new Color(0.2f, 0.2f, 0.2f, 1.0f),
                textDisabled = new Color(0.75f, 0.75f, 0.75f, 1.0f);
    public UnityEngine.UI.Text text;

    protected override bool IsSelected() {
        return false;
    }

    void Start() {
        prev = false;
    }

    void Update() {
        if(b == null) b = GetComponent<UnityEngine.UI.Button>();
        bool curr = BarManager.inst.td == (TDOption)number;
        if(prev ^ curr) {
            b.colors = curr ? selected : unselected;
            prev = curr;
        }
        switch((TDOption)number) {
            case TDOption.LG_SIX:
                b.interactable = BarManager.inst.BarSize > 1;
                text.color = b.interactable ? textEnabled : textDisabled;
                break;
            case TDOption.LG_SEVEN:
                b.interactable = BarManager.inst.BarSize > 2 && BarManager.inst.BarSize != 4;
                text.color = b.interactable ? textEnabled : textDisabled;
                break;
            case TDOption.LG_EIGHT:
                b.interactable = BarManager.inst.BarSize > 3;
                text.color = b.interactable ? textEnabled : textDisabled;
                break;
            default:
                text.color = textEnabled;
                break;
        }
    }

    public override void Clicked() {
    }
}
