using UnityEngine;
using System.Collections;

public class LargeFSBSTT : IssueChecker {

    public override bool DoCheck() {
        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(!lh.gameObject.activeInHierarchy) continue;
            if(lh.isSmall) continue;
            if(lh.lhd.funcs.Contains(BasicFunction.TAKEDOWN) || lh.lhd.funcs.Contains(BasicFunction.ALLEY) || lh.lhd.funcs.Contains(BasicFunction.STT)) {
                return true;
            }
        }

        return false;
    }
}
