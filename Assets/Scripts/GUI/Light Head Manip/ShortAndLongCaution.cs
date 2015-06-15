using UnityEngine;
using System.Collections;

public class ShortAndLongCaution : MonoBehaviour {
    public GameObject CautionObject;

    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) return;

        bool Short = true, Long = true;

        foreach(LightHead alpha in FindObjectsOfType<LightHead>()) {
            if(alpha.Selected) {
                Short &= alpha.isSmall;
                Long &= !alpha.isSmall;
            }
        }

        CautionObject.SetActive(!Short && !Long);
    }
}
