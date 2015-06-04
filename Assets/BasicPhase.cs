using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BasicPhase : MonoBehaviour {
    public bool IsPhaseB;
    public Image Check;

    void Update() {
        bool active = true;
        bool interact = false;
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected) {
                active &= IsPhaseB ? alpha.basicPhaseB : !alpha.basicPhaseB;
                interact |= alpha.lhd.funcs.Contains(BasicFunction.FLASHING);
            }
        }
        Check.enabled = active & interact;
        GetComponent<Button>().interactable = interact;
    }

    public void Clicked() {
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected && alpha.lhd.funcs.Contains(BasicFunction.FLASHING)) {
                alpha.basicPhaseB = IsPhaseB;
            }
        }
    }
}
