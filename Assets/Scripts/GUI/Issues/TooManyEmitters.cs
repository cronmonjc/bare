﻿using UnityEngine;
using System.Collections;

public class TooManyEmitters : IssueChecker {

    public override bool DoCheck() {
        int count = 0;

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy) {
                if(alpha.lhd.funcs.Contains(BasicFunction.EMITTER)) {
                    count++;
                    if(count == 2) return true;
                }
            }
        }
        return false;
    }
}