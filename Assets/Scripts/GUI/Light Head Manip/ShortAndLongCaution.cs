using UnityEngine;
using System.Collections;

public class ShortAndLongCaution : MonoBehaviour {
    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) return;

        bool Short = true, Long = true;

        foreach(LightHead alpha in FindObjectsOfType<LightHead>()) {
            if(alpha.Selected) {
                Short &= alpha.isSmall;
                Long &= !alpha.isSmall;
            }
        }

        GetComponent<UnityEngine.UI.Text>().enabled = (!Short && !Long);
    }
}
