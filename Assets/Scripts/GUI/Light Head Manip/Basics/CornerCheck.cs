using UnityEngine;
using System.Collections;

public class CornerCheck : MonoBehaviour {
    public GameObject CornerLong, RearCorner;

    public void Retest() {
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;

            CornerLong.SetActive(false);
            RearCorner.SetActive(false);

            if(alpha.loc == Location.FRONT_CORNER || alpha.loc == Location.REAR_CORNER) {
                CornerLong.SetActive(true);
            }
            if(alpha.loc == Location.REAR_CORNER) {
                RearCorner.SetActive(true);
            }
        }
    }
}
