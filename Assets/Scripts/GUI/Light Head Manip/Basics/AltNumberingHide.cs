using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI Component.  Hides the Staggered Output toggle when necessary.
/// </summary>
public class AltNumberingHide : MonoBehaviour {
    /// <summary>
    /// The reference to the GameObject containing the toggle Components.
    /// </summary>
    private GameObject child;

    /// <summary>
    /// Refreshes this Component.  Tests whether it should be shown.
    /// </summary>
    public void Refresh() {
        if(child == null) child = transform.GetChild(0).gameObject; // Get reference if we don't have one already

        child.SetActive(false); // Hide by default

        if(FunctionEditPane.currFunc != AdvFunction.NONE) return;

        Collider[] test = Physics.OverlapSphere(new Vector3(0, 1.25f, 0), 3.2f); // Creates a sphere, centered on the front center of the bar, and captures whatever LightHeads are there.
        Dictionary<string, LightHead> dict = new Dictionary<string, LightHead>();

        foreach(Collider col in test) {
            LightHead thisHead = col.GetComponent<LightHead>(); // Looks for and caches the heads found
            if(thisHead != null) dict[col.transform.GetPath()] = thisHead;
        }

        switch(BarManager.inst.BarSize) {
            case 2:
                if(dict.ContainsKey("/Bar/DF/F/L")) return; // Search for certain heads - if found, stop
                if(dict.ContainsKey("/Bar/PF/F/L")) return;

                if(!dict["/Bar/DF/F/DS/L"].hasRealHead) return; // Examine heads - if heads don't have real head, stop
                if(!dict["/Bar/DF/F/DS/R"].hasRealHead) return;
                if(!dict["/Bar/PF/F/DS/L"].hasRealHead) return;
                if(!dict["/Bar/PF/F/DS/R"].hasRealHead) return;

                if(dict["/Bar/DF/F/DS/L"].Selected || dict["/Bar/DF/F/DS/R"].Selected || dict["/Bar/PF/F/DS/L"].Selected || dict["/Bar/PF/F/DS/R"].Selected) {
                    StyleNode alpha = dict["/Bar/DF/F/DS/L"].lhd.style;
                    if(alpha != dict["/Bar/DF/F/DS/R"].lhd.style) return; // Examine styles - if they don't all match, stop
                    if(alpha != dict["/Bar/PF/F/DS/L"].lhd.style) return;
                    if(alpha != dict["/Bar/PF/F/DS/R"].lhd.style) return;

                    child.SetActive(true); // Enable display of the toggle
                }
                break;
            case 3:
                if(dict.ContainsKey("/Bar/DF/F/L")) return; // Search for certain heads - if found, stop
                if(dict.ContainsKey("/Bar/DN/F/L")) return;
                if(dict.ContainsKey("/Bar/PF/F/L")) return;

                if(dict["/Bar/DF/F/DS/L"].hasRealHead) return; // (This one returns if it has a real head)
                if(!dict["/Bar/DF/F/DS/R"].hasRealHead) return; // Examine heads - if heads don't have real head, stop
                if(!dict["/Bar/DN/F/DS/L"].hasRealHead) return;
                if(!dict["/Bar/DN/F/DS/R"].hasRealHead) return;
                if(!dict["/Bar/PF/F/DS/L"].hasRealHead) return;
                if(dict["/Bar/PF/F/DS/R"].hasRealHead) return; // (This one returns if it has a real head)

                if(dict["/Bar/DF/F/DS/R"].Selected || dict["/Bar/DN/F/DS/L"].Selected || dict["/Bar/DN/F/DS/R"].Selected || dict["/Bar/PF/F/DS/L"].Selected) {
                    StyleNode alpha = dict["/Bar/DF/F/DS/R"].lhd.style;
                    if(alpha != dict["/Bar/DN/F/DS/L"].lhd.style) return; // Examine styles - if they don't all match, stop
                    if(alpha != dict["/Bar/DN/F/DS/R"].lhd.style) return;
                    if(alpha != dict["/Bar/PF/F/DS/L"].lhd.style) return;

                    child.SetActive(true); // Enable display of the toggle
                }
                break;
            case 4:
                if(!dict.ContainsKey("/Bar/DF/F/L")) return; // Search for certain heads - if not found, stop
                if(!dict.ContainsKey("/Bar/DN/F/L")) return;
                if(!dict.ContainsKey("/Bar/PN/F/L")) return;
                if(!dict.ContainsKey("/Bar/PF/F/L")) return;

                if(!dict["/Bar/DF/F/L"].hasRealHead) return; // Examine heads - if heads don't have real head, stop
                if(!dict["/Bar/DN/F/L"].hasRealHead) return;
                if(!dict["/Bar/PN/F/L"].hasRealHead) return;
                if(!dict["/Bar/PF/F/L"].hasRealHead) return;

                if(dict["/Bar/DF/F/L"].Selected || dict["/Bar/DN/F/L"].Selected || dict["/Bar/PN/F/L"].Selected || dict["/Bar/PF/F/L"].Selected) {
                    StyleNode alpha = dict["/Bar/DF/F/L"].lhd.style;
                    if(alpha != dict["/Bar/DN/F/L"].lhd.style) return; // Examine styles - if they don't all match, stop
                    if(alpha != dict["/Bar/PN/F/L"].lhd.style) return;
                    if(alpha != dict["/Bar/PF/F/L"].lhd.style) return;

                    child.SetActive(true); // Enable display of the toggle
                }
                break;
            default:
                break;
        }
    }
}
