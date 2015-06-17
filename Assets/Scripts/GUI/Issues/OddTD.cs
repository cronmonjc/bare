using UnityEngine;
using System.Collections;

public class OddTD : IssueChecker {

    public override bool DoCheck() {
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.lhd.funcs.Contains(BasicFunction.TRAFFIC) && !alpha.shouldBeTD) {
                return true;
            }
        }
        return false;
    }
}
