using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AltNumberingHide : MonoBehaviour {
    private GameObject child;

    public void Refresh() {
        if(child == null) child = transform.GetChild(0).gameObject;

        child.SetActive(false);

        if(BarManager.inst.BarSize == 2) {
            Collider[] test = Physics.OverlapSphere(new Vector3(0, 1.25f, 0), 1.6f);
            Dictionary<string, LightHead> dict = new Dictionary<string, LightHead>();
            foreach(Collider col in test) {
                LightHead thisHead = col.GetComponent<LightHead>();
                if(thisHead != null) dict[col.transform.GetPath()] = thisHead;
            }
            if(!dict.ContainsKey("/Bar/DF/F/DS/L")) return;
            if(!dict.ContainsKey("/Bar/PF/F/DS/R")) return;

            if(!dict["/Bar/DF/F/DS/L"].hasRealHead) return;
            if(!dict["/Bar/DF/F/DS/R"].hasRealHead) return;
            if(!dict["/Bar/PF/F/DS/L"].hasRealHead) return;
            if(!dict["/Bar/PF/F/DS/R"].hasRealHead) return;

            if(dict["/Bar/DF/F/DS/L"].Selected || dict["/Bar/DF/F/DS/R"].Selected || dict["/Bar/PF/F/DS/L"].Selected || dict["/Bar/PF/F/DS/R"].Selected) {
                StyleNode alpha = dict["/Bar/DF/F/DS/L"].lhd.style;
                if(alpha != dict["/Bar/DF/F/DS/R"].lhd.style) return;
                if(alpha != dict["/Bar/PF/F/DS/L"].lhd.style) return;
                if(alpha != dict["/Bar/PF/F/DS/R"].lhd.style) return;

                child.SetActive(true);
            }
        } else if(BarManager.inst.BarSize == 4) {
            Collider[] test = Physics.OverlapSphere(new Vector3(0, 1.25f, 0), 3.2f);
            Dictionary<string, LightHead> dict = new Dictionary<string, LightHead>();
            foreach(Collider col in test) {
                LightHead thisHead = col.GetComponent<LightHead>();
                if(thisHead != null) dict[col.transform.GetPath()] = thisHead;
            }
            if(!dict.ContainsKey("/Bar/DF/F/L")) return;
            if(!dict.ContainsKey("/Bar/DN/F/L")) return;
            if(!dict.ContainsKey("/Bar/PN/F/L")) return;
            if(!dict.ContainsKey("/Bar/PF/F/L")) return;

            if(!dict["/Bar/DF/F/L"].hasRealHead) return;
            if(!dict["/Bar/DN/F/L"].hasRealHead) return;
            if(!dict["/Bar/PN/F/L"].hasRealHead) return;
            if(!dict["/Bar/PF/F/L"].hasRealHead) return;

            if(dict["/Bar/DF/F/L"].Selected || dict["/Bar/DN/F/L"].Selected || dict["/Bar/PN/F/L"].Selected || dict["/Bar/PF/F/L"].Selected) {
                StyleNode alpha = dict["/Bar/DF/F/L"].lhd.style;
                if(alpha != dict["/Bar/DN/F/L"].lhd.style) return;
                if(alpha != dict["/Bar/PN/F/L"].lhd.style) return;
                if(alpha != dict["/Bar/PF/F/L"].lhd.style) return;

                child.SetActive(true);
            }
        }
    }
}
