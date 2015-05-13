using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SplitMerge : MonoBehaviour {
    public static CameraControl cam;

    public bool IsMerge;

    void Update() {
        if(cam == null) cam = FindObjectOfType<CameraControl>();
        if(CameraControl.funcBeingTested != AdvFunction.NONE) return;
        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(!lh.gameObject.activeInHierarchy || !lh.Selected) continue;
            SizeOptionControl soc = null;
            for(Transform t = lh.transform; soc == null && t != null; t = t.parent) {
                soc = t.GetComponent<SizeOptionControl>();
            }
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

    public void Act(bool merge) {
        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(!lh.gameObject.activeInHierarchy || !lh.Selected) continue;
            SizeOptionControl soc = null;
            for(Transform t = lh.transform; soc == null && t != null; t = t.parent) {
                soc = t.GetComponent<SizeOptionControl>();
            }
            if(soc == null) {
                continue;
            }

            soc.ShowLong = merge;
        }
        cam.fs.Refresh();
    }
}
