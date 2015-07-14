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
    public bool refreshedThisFrame = false;

    public void Clear() {
        List<Transform> temp = new List<Transform>();
        foreach(Transform alpha in menu) {
            temp.Add(alpha);
        }
        foreach(Transform alpha in temp) {
            Destroy(alpha.gameObject);
        }
    }

    public void CreateButtons() {
        Clear();

        if(IsTD) {
            foreach(Pattern alpha in LightDict.inst.tdPatts) {
                GameObject newbie = GameObject.Instantiate<GameObject>(optionPrefab);
                newbie.transform.SetParent(menu, false);
                newbie.transform.localScale = Vector3.one;
                FuncPatt patt = newbie.GetComponent<FuncPatt>();
                patt.fps = this;
                patt.patt = alpha;
            }
        } else {
            foreach(Pattern alpha in LightDict.inst.flashPatts) {
                GameObject newbie = GameObject.Instantiate<GameObject>(optionPrefab);
                newbie.transform.SetParent(menu, false);
                newbie.transform.localScale = Vector3.one;
                FuncPatt patt = newbie.GetComponent<FuncPatt>();
                patt.fps = this;
                patt.patt = alpha;
            }
        }
    }

    public void Refresh() {
        if(refreshedThisFrame) return;
        if(menu.childCount == 0) {
            CreateButtons();
        }

        bool showPatts = false;
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;

            switch(FunctionEditPane.currFunc) {
                case AdvFunction.TRAFFIC_LEFT:
                case AdvFunction.TRAFFIC_RIGHT:
                    showPatts |= alpha.lhd.funcs.Contains(BasicFunction.TRAFFIC) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.PRIO1:
                case AdvFunction.PRIO2:
                case AdvFunction.PRIO3:
                case AdvFunction.PRIO4:
                case AdvFunction.PRIO5:
                case AdvFunction.FTAKEDOWN:
                case AdvFunction.FALLEY:
                case AdvFunction.ICL:
                    showPatts |= alpha.lhd.funcs.Contains(BasicFunction.FLASHING) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                default:
                    break;
            }

            if(showPatts)
                alpha.PrefetchPatterns(FunctionEditPane.currFunc);
        }

        menu.gameObject.SetActive(showPatts);

        if(showPatts) {
            foreach(FuncPatt alpha in menu.GetComponentsInChildren<FuncPatt>(true)) {
                alpha.Refresh();
            }
        }
        refreshedThisFrame = true;
    }

    public void SetSelection(Pattern p) {
        string cmpdName = BarManager.GetFnString(transform, FunctionEditPane.currFunc);
        if(cmpdName == null) {
            Debug.LogWarning("lolnope - " + FunctionEditPane.currFunc.ToString() + " has no similar setting in the data bytes.");
            return;
        }
        if(FunctionEditPane.currFunc == AdvFunction.TRAFFIC_LEFT || FunctionEditPane.currFunc == AdvFunction.TRAFFIC_RIGHT) {
            BarManager.inst.patts.Get<NbtCompound>(cmpdName).Get<NbtShort>("patt").Value = (short)p.id;

            foreach(LightLabel ll in FindObjectsOfType<LightLabel>()) {
                ll.Refresh();
            }

            foreach(FuncEnable fe in FindObjectsOfType<FuncEnable>()) {
                if(!(fe.IsColor2 ^ IsColor2)) {
                    fe.Enable();
                }
            }
        } else {
            NbtCompound patCmpd = BarManager.inst.patts.Get<NbtCompound>(cmpdName).Get<NbtCompound>("pat" + (IsColor2 ? "2" : "1"));

            foreach(LightHead alpha in cam.OnlyCamSelectedHead) {
                string tagname = alpha.transform.position.y < 0 ? "r" : "f";
                string path = alpha.Path;

                if(path.Contains("C") || path.Contains("A")) {
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

                if(p is WarnPatt || p is DCCirclePattern || p is DCDoubleRotatorPattern) {
                    ((NbtCompound)patCmpd.Parent).Get<NbtShort>("p" + (alpha.isRear ? "r" : "f") + (IsColor2 ? "2" : "1")).DisableBit(alpha.Bit);
                }

                switch(FunctionEditPane.currFunc) {
                    case AdvFunction.FTAKEDOWN:
                    case AdvFunction.FALLEY:
                    case AdvFunction.ICL:
                        if(!alpha.GetIsEnabled(FunctionEditPane.currFunc, !IsColor2)) {
                            NbtCompound otherPatCmpd = ((NbtCompound)patCmpd.Parent).Get<NbtCompound>("pat" + (IsColor2 ? "1" : "2"));

                            otherPatCmpd.Get<NbtShort>(tagname).Value = (short)p.id;

                            NbtShort thisPhase = ((NbtCompound)patCmpd.Parent).Get<NbtShort>("p" + (alpha.isRear ? "r" : "f") + (IsColor2 ? "2" : "1")),
                                    otherPhase = ((NbtCompound)patCmpd.Parent).Get<NbtShort>("p" + (alpha.isRear ? "r" : "f") + (IsColor2 ? "1" : "2"));

                            if((thisPhase.Value & (0x1 << alpha.Bit)) > 0) {
                                otherPhase.DisableBit(alpha.Bit);
                            } else {
                                otherPhase.EnableBit(alpha.Bit);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            foreach(LightLabel ll in FindObjectsOfType<LightLabel>()) {
                ll.Refresh();
            }

            foreach(FuncEnable fe in FindObjectsOfType<FuncEnable>()) {
                if(!(fe.IsColor2 ^ IsColor2)) {
                    fe.Enable();
                }
            }
        }

        FunctionEditPane.RetestStatic();
        BarManager.moddedBar = true;
        if(BarManager.inst.patts.Contains("prog")) BarManager.inst.patts.Remove("prog");
    }

    void LateUpdate() {
        refreshedThisFrame = false;
    }
}
