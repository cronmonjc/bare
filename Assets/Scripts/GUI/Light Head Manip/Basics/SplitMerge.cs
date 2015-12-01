using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI Component.  Gives the user control over SizeOptionControls, letting them control the size of the light heads.
/// </summary>
public class SplitMerge : MonoBehaviour {
    /// <summary>
    /// Reference to the CameraControl, to get access to the Function Select to refresh it
    /// </summary>
    public static CameraControl cam;

    /// <summary>
    /// Is this Component merging heads?  If not, it's splitting.  Set via Unity Inspector.
    /// </summary>
    public bool IsMerge;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(cam == null) cam = FindObjectOfType<CameraControl>();
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) return;  // Quick & dirty optimization - if we're previewing a function, do nothing
        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(!lh.gameObject.activeInHierarchy || !lh.Selected) continue;
            SizeOptionControl soc = lh.soc;
            if(soc == null) {
                continue;
            }

            if ((IsMerge && !soc.ShowLong && soc.canLong[BarManager.inst.BarSize]) || (!IsMerge && soc.ShowLong && soc.canShort[BarManager.inst.BarSize])) {
                GetComponent<Button>().interactable = true;
                return;
            }
        }
        GetComponent<Button>().interactable = false;
    }

    /// <summary>
    /// Performs the merge or split.
    /// </summary>
    public void Act() {
        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(!lh.gameObject.activeInHierarchy || !lh.Selected) continue;
            SizeOptionControl soc = lh.soc;
            if(soc == null) {
                continue;
            }

            if(soc.ShowLong != IsMerge) {
                soc.ShowLong = IsMerge;
            }
        }
        cam.fs.Refresh();
        BarManager.inst.StartCoroutine(BarManager.inst.RefreshBitsIEnum());
        StartCoroutine(BarManager.inst.RefreshAllLabels());
        BarManager.moddedBar = true;
    }

}
