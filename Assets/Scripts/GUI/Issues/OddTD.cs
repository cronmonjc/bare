using UnityEngine;
using System.Collections;

public class OddTD : IssueChecker {

    public override bool DoCheck() {
        RaycastHit[] hits = Physics.RaycastAll(new Vector3(-9, -1.25f, 0), new Vector3(1, 0, 0));

        bool small = false, large = false;
        int count = 0;
        foreach(RaycastHit hit in hits) {
            LightHead alpha = hit.transform.GetComponent<LightHead>();
            if(alpha != null && alpha.lhd.funcs.Contains(BasicFunction.TRAFFIC)) {
                small |= alpha.isSmall;
                large |= !alpha.isSmall;

                count++;
            }
        }

        if(!small && !large) return false;  // No TD = no issue
        if(small && large) return true;  // Both sizes = issue

        return ((small && (count != 6 && count != 8)) || (large && count != 7));
    }
}
