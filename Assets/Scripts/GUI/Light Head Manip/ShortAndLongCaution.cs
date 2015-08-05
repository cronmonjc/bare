using UnityEngine;
using System.Collections;

public class ShortAndLongCaution : MonoBehaviour {
    public GameObject CautionObject;

    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) return;  // Quick & dirty optimization - if we're previewing a function, do nothing

        bool Short = true, Long = true;

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected) {
                Short &= alpha.isSmall;
                Long &= !alpha.isSmall;
            }
        }

        CautionObject.SetActive(!Short && !Long);
    }
}
