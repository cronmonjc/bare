using UnityEngine;
using System.Collections;

public class NonLightTD : IssueChecker {

    public override bool DoCheck() {
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.shouldBeTD && !alpha.hasRealHead) return true;
        }
        return false;
    }
}
