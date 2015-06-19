using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BasicPhase : MonoBehaviour {
    public bool IsPhaseB;
    public bool IsColor2;
    public Image Check;
    private Button b;

    void Update() {
        bool active = true;
        bool interact = false;
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected) {
                if(IsColor2) active &= IsPhaseB ? alpha.basicPhaseB2 : !alpha.basicPhaseB2;
                else active &= IsPhaseB ? alpha.basicPhaseB : !alpha.basicPhaseB;
                interact |= alpha.lhd.funcs.Contains(BasicFunction.FLASHING) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
            }
        }
        Check.enabled = active & interact;
        if(b == null) b = GetComponent<Button>();
        b.interactable = interact;
    }

    public void Clicked() {
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected && alpha.lhd.funcs.Contains(BasicFunction.FLASHING)) {
                if(IsColor2) alpha.basicPhaseB2 = IsPhaseB;
                else alpha.basicPhaseB = IsPhaseB;
            }
        }
        BarManager.moddedBar = true;
    }
}
