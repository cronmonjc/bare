using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SplitMerge : MonoBehaviour {
    public static CameraControl cam;

    public bool IsMerge;

    void Update() {
        if(cam == null) cam = FindObjectOfType<CameraControl>();
        foreach(LightHead lh in cam.OnlyCamSelected) {
            SizeOptionControl soc = lh.GetComponentInParent<SizeOptionControl>();
            if(soc == null) {
                soc = lh.transform.parent.GetComponentInParent<SizeOptionControl>();
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
        List<LightHead> temp = new List<LightHead>(cam.OnlyCamSelected);
        foreach(LightHead lh in temp) {
            SizeOptionControl soc = lh.GetComponentInParent<SizeOptionControl>();
            if(soc == null) {
                soc = lh.transform.parent.GetComponentInParent<SizeOptionControl>();
            }
            if(soc == null) {
                continue;
            }

            soc.ShowLong = merge;
            cam.Selected.Remove(lh);
            foreach(LightHead alpha in soc.transform.GetComponentsInChildren<LightHead>(false)) {
                if(!cam.Selected.Contains(alpha)) {
                    cam.Selected.Add(alpha);
                    alpha.selectedFunctions.Clear();
                    foreach(Function f in lh.selectedFunctions) {
                        if(alpha.CapableFunctions.Contains(f) && !alpha.selectedFunctions.Contains(f)) {
                            alpha.selectedFunctions.Add(f);
                        }
                    }
                    if(lh.lhd.optic != null) {
                        if(merge) {
                            if(lh.lhd.optic.lgEquivalent.Length > 0) {
                                alpha.SetOptic(lh.lhd.optic.lgEquivalent, false);
                                alpha.SetStyle(lh.lhd.style);
                            }
                        } else {
                            if(lh.lhd.optic.smEquivalent.Length > 0) {
                                alpha.SetOptic(lh.lhd.optic.smEquivalent, false);
                                alpha.SetStyle(lh.lhd.style);
                            }
                        }
                    }
                }
            }
        }
        cam.os.Refresh();
        foreach(FunctionSelect alpha in cam.FuncSelectRoot.GetComponentsInChildren<FunctionSelect>(true)) {
            alpha.Refresh();
        }
    }
}
