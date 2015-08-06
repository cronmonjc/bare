using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SplitMerge : MonoBehaviour {
    public static CameraControl cam;

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

            if((IsMerge && !soc.ShowLong) || (!IsMerge && soc.ShowLong)) {
                GetComponent<Button>().interactable = true;
                return;
            }
        }
        GetComponent<Button>().interactable = false;
    }

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
