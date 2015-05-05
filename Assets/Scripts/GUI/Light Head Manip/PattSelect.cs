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

        GameObject newbie = GameObject.Instantiate<GameObject>(prefab);
        newbie.transform.SetParent(menu, false);
        newbie.transform.localScale = Vector3.one;
        PattSelectElement newpse = newbie.GetComponent<PattSelectElement>();
        newpse.selID = -1;
        newpse.ps = this;
        newpse.Function = "Disabled";

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

    public void Select(PattSelectElement pse) {
        PattSelectElement[] allpse = GetComponentsInChildren<PattSelectElement>();

        foreach(PattSelectElement alpha in allpse) {
            alpha.PattActive = (alpha == pse);
        }

        NbtCompound patts = FindObjectOfType<BarManager>().patts;
        foreach(LightHead lh in FindObjectsOfType<LightHead>()) {
            if(!lh.gameObject.activeInHierarchy || !lh.Selected) continue;
            string cmpdName = BarManager.GetFnString(lh, f);
            if(cmpdName == null) {
                Debug.Log("lolnope - " + f.ToString() + " has no similar setting in the data bytes.  Ask James.");
                ErrorText.inst.DispError("lolnope - " + f.ToString() + " has no similar setting in the data bytes.  Ask James.");
                continue;
            }
            NbtCompound func = patts.Get<NbtCompound>(cmpdName);


        }
    }

    public void DisableEnable(bool IsEnable) {
        NbtCompound patts = FindObjectOfType<BarManager>().patts;
        foreach(LightBlock lb in FindObjectsOfType<LightBlock>()) {
            if(!lb.gameObject.activeInHierarchy || !lb.Selected) continue;
            LightHead lh = null;
            for(Transform t = lb.transform; lb == null && t != null; t = t.parent) {
                lh = t.GetComponent<LightHead>();
            }
            if(lh == null) {
                Debug.LogError("lolnope - " + lb.GetPath() + " can't find a LightHead.");
                ErrorText.inst.DispError(lb.GetPath() + " can't find a LightHead.");
                continue;
            }

            string cmpdName = BarManager.GetFnString(lh, f);
            if(cmpdName == null) {
                Debug.LogWarning("lolnope - " + f.ToString() + " has no similar setting in the data bytes.  Ask James.");
                ErrorText.inst.DispError(f.ToString() + " has no similar setting in the data bytes.  Ask James.");
                return;
            }
            NbtCompound func = patts.Get<NbtCompound>(cmpdName);

            NbtShort en = func.Get<NbtShort>("en" + (lh.transform.position.z > 0 ? "f" : "r") + (lh.DualR == lb ? "2" : "1"));


        }
    }
}
