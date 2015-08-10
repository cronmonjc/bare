using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI Component, Issue.  Checks to make sure certain functions aren't missing from the input map when they're used on the bar itself.
/// </summary>
public class UnwiredFunc : IssueChecker {

    /// <summary>
    /// The current frame's list of missing functions as a bit field.  Prevents having to repopulate a few mildly expensive variables.
    /// </summary>
    private int thisRun;
    /// <summary>
    /// The currently generated list of missing functions as a bit field.  Prevents having to repopulate a few mildly expensive variables.
    /// </summary>
    private int missingFuncInt;
    /// <summary>
    /// The list of missing functions
    /// </summary>
    private List<string> missingFuncList;
    /// <summary>
    /// The last function of the missing function list, used to insert an "and"
    /// </summary>
    private string lastFunc;

    /// <summary>
    /// The list of missing functions in textual form, for use in injection into the issue text
    /// </summary>
    private string functions;

    /// <summary>
    /// Local cache of the input map.
    /// </summary>
    private List<int> map;

    public override string pdfText {
        get {
            return "This bar contains " + (missingFuncList.Count > 1 ? "specific functions - " : "a specific function - ") + functions + " - on at least one of the heads on this bar, " +
                   "but the input map for this bar is missing a specified input for that function. The functions in use are listed on the fourth page of this document, directly under the diagram of the bar.";
        }
    }

    public override bool DoCheck() {
        #region (Re)cache map
        if(map == null) map = new List<int>(FnDragTarget.inputMap.Value);
        else {
            map.Clear();
            map.AddRange(FnDragTarget.inputMap.Value);
        } 
        #endregion

        // Init this run
        thisRun = 0;

        #region Search for missing functions
        for(byte h = 0; h < BarManager.inst.allHeads.Count; h++) {
            LightHead alpha = BarManager.inst.allHeads[h];
            if(!alpha.gameObject.activeInHierarchy) continue;
            for(byte i = 0; i < alpha.lhd.funcs.Count; i++) {
                switch(alpha.lhd.funcs[i]) {
                    case BasicFunction.CAL_STEADY:
                        if(!map.Contains(0x20000) && (thisRun & 0x20000) == 0) {
                            thisRun |= 0x20000;
                            missingFuncList.Add("California Title 13 Steady");
                        }
                        break;
                    case BasicFunction.CRUISE:
                        if(!map.Contains(0x2000) && (thisRun & 0x2000) == 0) {
                            thisRun |= 0x2000;
                            missingFuncList.Add("Cruise");
                        }
                        break;
                    case BasicFunction.EMITTER:
                        if(!map.Contains(0x100000) && (thisRun & 0x100000) == 0) {
                            thisRun |= 0x100000;
                            missingFuncList.Add("Emitter");
                        }
                        break;
                    case BasicFunction.STT:
                        if((!map.Contains(0x4000) || !map.Contains(0x8000) || !map.Contains(0x10000)) && (thisRun & 0x1C000) == 0) {
                            thisRun |= 0x1C000;
                            missingFuncList.Add("Stop/Tail/Turn");
                        }
                        break;
                    case BasicFunction.TRAFFIC:
                        if((!map.Contains(0x10) || !map.Contains(0x20)) && (thisRun & 0x30) == 0) {
                            thisRun |= 0x30;
                            missingFuncList.Add("Traffic Director");
                        }
                        break;
                    default:
                        break;
                }
            }
        } 
        #endregion

        #region Test if we already had this list generated
        if(thisRun == missingFuncInt) {
            return missingFuncInt != 0;
        } 
        #endregion

        #region Clear current missing function list
        if(missingFuncList == null) missingFuncList = new List<string>();
        else {
            missingFuncList.Clear();
        } 
        #endregion

        missingFuncInt = thisRun;

        #region Generate the single-string list of functions
        if(missingFuncList.Count > 0) {
            if(missingFuncList.Count > 1) {
                if(missingFuncList.Count == 2) {
                    functions = string.Join(" and ", missingFuncList.ToArray());
                } else {
                    lastFunc = missingFuncList[missingFuncList.Count - 1];
                    missingFuncList.RemoveAt(missingFuncList.Count - 1);
                    functions = ", and " + lastFunc;
                    functions = string.Join(", ", missingFuncList.ToArray()) + functions;
                }
                text.text = "You're using specific functions - " + functions + " - on at least one of the above heads, however an input for that function hasn't been assigned. We suggest that you examine the input map by clicking \"Pattern Editing\" above and assigning the function to an input.";
                missingFuncList.Add(lastFunc);
            } else {
                lastFunc = "";
                functions = missingFuncList[0];
                text.text = "You're using a specific function - " + functions + " - on at least one of the above heads, however an input for that function hasn't been assigned. We suggest that you examine the input map by clicking \"Pattern Editing\" above and assigning the function to an input.";
            }
            return true;
        }
        return false; 
        #endregion
    }
}
