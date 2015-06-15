using UnityEngine;
using System.Collections;

public class TDPulldown : PulldownItem {
    protected override bool IsSelected() {
        return BarManager.inst.td == (TDOption)number;
    }

    void Update() {
        if(b == null) b = GetComponent<UnityEngine.UI.Button>();
        switch((TDOption)number) {
            case TDOption.LG_SIX:
                b.interactable = BarManager.inst.BarSize > 1;
                break;
            case TDOption.LG_SEVEN:
                b.interactable = BarManager.inst.BarSize > 2 && BarManager.inst.BarSize != 4;
                break;
            case TDOption.LG_EIGHT:
                b.interactable = BarManager.inst.BarSize > 3;
                break;
            default:
                break;
        }
    }

    public override void Clicked() {
    }
}
