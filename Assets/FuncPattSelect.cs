using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using fNbt;

public class FuncPattSelect : MonoBehaviour {
    public RectTransform menu;
    public GameObject optionPrefab;
    public CameraControl cam;
    public bool IsTD = false;
    public bool IsColor2 = false;

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

        bool showPatts = false;
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;

            switch(FunctionEditPane.currFunc) {
                case AdvFunction.TRAFFIC_LEFT:
                case AdvFunction.TRAFFIC_RIGHT:
                    showPatts |= alpha.lhd.funcs.Contains(BasicFunction.TRAFFIC) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.LEVEL1:
                case AdvFunction.LEVEL2:
                case AdvFunction.LEVEL3:
                case AdvFunction.LEVEL4:
                case AdvFunction.LEVEL5:
                case AdvFunction.FTAKEDOWN:
                case AdvFunction.FALLEY:
                case AdvFunction.ICL:
                    showPatts |= alpha.lhd.funcs.Contains(BasicFunction.FLASHING) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                default:
                    break;
            }
        }

        if(!showPatts) return;

        if(IsTD) {
            foreach(Pattern alpha in LightDict.inst.tdPatts) {
                GameObject newbie = GameObject.Instantiate<GameObject>(optionPrefab);
                newbie.transform.SetParent(menu, false);
                newbie.transform.localScale = Vector3.one;
                FuncPatt patt = newbie.GetComponent<FuncPatt>();
                patt.fps = this;
                patt.patt = alpha;
                patt.Refresh();
            }

        } else {
            foreach(Pattern alpha in LightDict.inst.flashPatts) {
                GameObject newbie = GameObject.Instantiate<GameObject>(optionPrefab);
                newbie.transform.SetParent(menu, false);
                newbie.transform.localScale = Vector3.one;
                FuncPatt patt = newbie.GetComponent<FuncPatt>();
                patt.fps = this;
                patt.patt = alpha;
                patt.Refresh();
            }
            foreach(Pattern alpha in LightDict.inst.warnPatts) {
                GameObject newbie = GameObject.Instantiate<GameObject>(optionPrefab);
                newbie.transform.SetParent(menu, false);
                newbie.transform.localScale = Vector3.one;
                FuncPatt patt = newbie.GetComponent<FuncPatt>();
                patt.fps = this;
                patt.patt = alpha;
                patt.Refresh();
            }
        }
    }

    public void SetSelection(Pattern p) {
        string cmpdName = BarManager.GetFnString(transform, FunctionEditPane.currFunc);
        if(cmpdName == null) {
            Debug.LogWarning("lolnope - " + FunctionEditPane.currFunc.ToString() + " has no similar setting in the data bytes.");
            return;
        }
        NbtCompound patCmpd = BarManager.inst.patts.Get<NbtCompound>(cmpdName).Get<NbtCompound>("pat" + (IsColor2 ? "2" : "1"));

        foreach(LightHead alpha in cam.OnlyCamSelected) {
            string tagname = alpha.transform.position.z < 0 ? "r" : "f";
            string path = alpha.transform.GetPath();

            if(path.Contains("C")) {
                tagname = tagname + "cor";
            } else if(path.Contains("I")) {
                tagname = tagname + "inb";
            } else if(path.Contains("O")) {
                if(alpha.loc == Location.FAR_REAR)
                    tagname = tagname + "far";
                else
                    tagname = tagname + "oub";
            } else if(path.Contains("N") || path.Split('/')[2].EndsWith("F")) {
                tagname = tagname + "cen";
            }

            patCmpd.Get<NbtShort>(tagname).Value = (short)p.id;
        }

        foreach(LightLabel ll in FindObjectsOfType<LightLabel>()) {
            ll.Refresh();
        }

        FunctionEditPane.RetestStatic();
    }
}
