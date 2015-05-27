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

            if(soc.ShowLong != IsMerge) {
                soc.ShowLong = IsMerge;

                foreach(LightHead alpha in soc.GetComponentsInChildren<LightHead>(true)) {
                    if(alpha == lh) continue;

                    List<BasicFunction> potential = new List<BasicFunction>();
                    potential.Add(BasicFunction.FLASHING);
                    switch(alpha.loc) {
                        case Location.ALLEY:
                            potential.Add(BasicFunction.ALLEY);
                            break;
                        case Location.FRONT:
                            potential.Add(BasicFunction.TAKEDOWN);
                            potential.Add(BasicFunction.CAL_STEADY);
                            if(!alpha.isSmall) potential.Add(BasicFunction.EMITTER);
                            break;
                        case Location.REAR:
                            potential.Add(BasicFunction.TAKEDOWN);
                            potential.Add(BasicFunction.TRAFFIC);
                            break;
                        case Location.FAR_REAR:
                            potential.Add(BasicFunction.TAKEDOWN);
                            potential.Add(BasicFunction.STT);
                            break;
                        case Location.FRONT_CORNER:
                        case Location.REAR_CORNER:
                            potential.Add(BasicFunction.TAKEDOWN);
                            potential.Add(BasicFunction.CRUISE);
                            break;
                    }

                    alpha.lhd.funcs.Clear();
                    foreach(BasicFunction f in lh.lhd.funcs) {
                        if(potential.Contains(f)) {
                            alpha.lhd.funcs.Add(f);
                        }
                    }
                }
            }
        }
        cam.fs.Refresh();
        StartCoroutine(BarManager.inst.RefreshAllLabels());
    }

}
