using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnwiredFunc : IssueChecker {

    private List<int> map;

    public override string pdfText {
        get { return "This bar contains at least one specific function - Traffic Director, Stop/Tail/Turn, Cruise, Emitter, or California Title 13 Steady - on at least one of the heads on this bar, " +
                     "but the input map for this bar is missing a specified input for that function. The functions in use are listed on the fourth page of this document, directly under the diagram of the bar."; }
    }

    public override bool DoCheck() {
        if(map == null) map = new List<int>(FnDragTarget.inputMap.Value);
        else {
            map.Clear();
            map.AddRange(FnDragTarget.inputMap.Value);
        }

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy) continue;
            for(byte i = 0; i < alpha.lhd.funcs.Count; i++) {
                switch(alpha.lhd.funcs[i]) {
                    case BasicFunction.CAL_STEADY:
                        if(!map.Contains(0x20000)) return true;
                        break;
                    case BasicFunction.CRUISE:
                        if(!map.Contains(0x2000)) return true;
                        break;
                    case BasicFunction.EMITTER:
                        if(!map.Contains(0x100000)) return true;
                        break;
                    case BasicFunction.STT:
                        if(!map.Contains(0x4000) || !map.Contains(0x8000) || !map.Contains(0x10000)) return true;
                        break;
                    case BasicFunction.TRAFFIC:
                        if(!map.Contains(0x10) || !map.Contains(0x20)) return true;
                        break;
                    default:
                        break;
                }
            }
        }

        return false;
    }
}
