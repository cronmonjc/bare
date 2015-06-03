using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BasicPhase : MonoBehaviour {
    public bool IsPhaseB;
    public Image Check;

    void Update() {
        bool active = true;
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected) {
                active &= IsPhaseB ? alpha.basicPhaseB : !alpha.basicPhaseB;
            }
        }
        Check.enabled = active;
    }

    public void Clicked() {
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected) {
                alpha.basicPhaseB = IsPhaseB;
            }
        }
    }
}
