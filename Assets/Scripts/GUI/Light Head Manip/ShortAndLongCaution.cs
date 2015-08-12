using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Deprecated.  Used to display a warning when short and long heads were selected, warning that selections might be limited (no longer the case).
/// </summary>
public class ShortAndLongCaution : MonoBehaviour {
    public GameObject CautionObject;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
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
