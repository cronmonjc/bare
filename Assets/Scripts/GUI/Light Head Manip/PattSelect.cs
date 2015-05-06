using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using fNbt;

public class PattSelect : MonoBehaviour {
    public Function f = Function.NONE;
    public RectTransform menu;
    public GameObject prefab;
    private CameraControl cam;

    public void Clear() {
        List<Transform> temp = new List<Transform>();
        foreach(Transform alpha in menu) {
            temp.Add(alpha);
        }
        foreach(Transform alpha in temp) {
            DestroyImmediate(alpha.gameObject);
        }
    }

    public void Refresh() {
        Clear();

        GameObject newbie;
        PattSelectElement newpse;

        if(LightDict.inst.steadyBurn.Contains(f)) {
            newbie = GameObject.Instantiate<GameObject>(prefab);
            newbie.transform.SetParent(menu, false);
            newbie.transform.localScale = Vector3.one;
            newpse = newbie.GetComponent<PattSelectElement>();
            newpse.selID = 0;
            newpse.ps = this;
            newpse.Function = "Enabled";
        } else if(f == Function.TRAFFIC) {
            foreach(Pattern p in LightDict.inst.tdPatts) {
                newbie = GameObject.Instantiate<GameObject>(prefab);
                newbie.transform.SetParent(menu, false);
                newbie.transform.localScale = Vector3.one;
                newpse = newbie.GetComponent<PattSelectElement>();
                newpse.selID = (short)p.id;
                newpse.ps = this;
                newpse.Function = p.name;
            }
        } else {
            foreach(Pattern p in LightDict.inst.flashPatts) {
                newbie = GameObject.Instantiate<GameObject>(prefab);
                newbie.transform.SetParent(menu, false);
                newbie.transform.localScale = Vector3.one;
                newpse = newbie.GetComponent<PattSelectElement>();
                newpse.selID = (short)p.id;
                newpse.ps = this;
                newpse.Function = p.name;
            }
            foreach(Pattern p in LightDict.inst.warnPatts) {
                newbie = GameObject.Instantiate<GameObject>(prefab);
                newbie.transform.SetParent(menu, false);
                newbie.transform.localScale = Vector3.one;
                newpse = newbie.GetComponent<PattSelectElement>();
                newpse.selID = (short)p.id;
                newpse.ps = this;
                newpse.Function = p.name;
            }
        }
    }
}
