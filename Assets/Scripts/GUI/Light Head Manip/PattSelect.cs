using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using fNbt;

public class PattSelect : MonoBehaviour {
    public static PattSelect inst;

    public AdvFunction f = AdvFunction.NONE;
    public RectTransform menu, dimSelect;
    public GameObject prefab;

    public DisableEnablePatt disableButton, enableButton;
    public PhaseButton PhaseA, PhaseB;

    void Awake() {
        if(inst == null) inst = this;
    }

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

        disableButton.Retest();
        enableButton.Retest();

        PhaseA.Retest();
        PhaseB.Retest();

        menu.gameObject.SetActive(true);
        dimSelect.gameObject.SetActive(false);

        if(LightDict.inst.steadyBurn.Contains(f)) {
            enableButton.text = "Enable Steady Burn";
            enableButton.GetComponent<Button>().interactable = true;

            PhaseA.GetComponent<Button>().interactable = false;
            PhaseB.GetComponent<Button>().interactable = false;
        } else if(f == AdvFunction.DIM) {
            menu.gameObject.SetActive(false);
            dimSelect.gameObject.SetActive(true);

            enableButton.text = "Enable Light Dimming";
            enableButton.GetComponent<Button>().interactable = true;

            PhaseA.GetComponent<Button>().interactable = false;
            PhaseB.GetComponent<Button>().interactable = false;
        //} else if(f == AdvFunction.TRAFFIC) {
        //    short selID = -2;
        //    foreach(Pattern p in LightDict.inst.tdPatts) {
        //        newbie = GameObject.Instantiate<GameObject>(prefab);
        //        newbie.transform.SetParent(menu, false);
        //        newbie.transform.localScale = Vector3.one;
        //        newpse = newbie.GetComponent<PattSelectElement>();
        //        newpse.selID = (short)p.id;
        //        newpse.ps = this;
        //        newpse.FuncText = p.name;
        //        newpse.Retest();
        //        if(TestPatternAny((short)p.id)) {
        //            if(selID == -2) {
        //                selID = (short)p.id;
        //            } else if(selID != -1 && selID != p.id) {
        //                selID = -1;
        //            }
        //        }
        //    }
        //    if(selID == -2) {
        //        enableButton.text = "Cannot Enable, No Pattern";
        //        enableButton.GetComponent<Button>().interactable = false;
        //    } else if(selID == -1) {
        //        enableButton.text = "Enable: <i>Multiple Patterns</i>";
        //        enableButton.GetComponent<Button>().interactable = true;
        //    } else {
        //        Pattern a = null;
        //        foreach(Pattern p in LightDict.inst.tdPatts) {
        //            if(p.id == selID) {
        //                a = p;
        //                break;
        //            }
        //        }
        //        if(a == null) {
        //            enableButton.text = "Cannot Enable, No Pattern";
        //            enableButton.GetComponent<Button>().interactable = false;
        //        } else {
        //            enableButton.text = "Enable: " + a.name;
        //            enableButton.GetComponent<Button>().interactable = true;
        //        }
        //    }

        //    PhaseA.GetComponent<Button>().interactable = false;
        //    PhaseB.GetComponent<Button>().interactable = false;
        } else {
            short selID = -2;
            foreach(Pattern p in LightDict.inst.flashPatts) {
                newbie = GameObject.Instantiate<GameObject>(prefab);
                newbie.transform.SetParent(menu, false);
                newbie.transform.localScale = Vector3.one;
                newpse = newbie.GetComponent<PattSelectElement>();
                newpse.selID = (short)p.id;
                newpse.ps = this;
                newpse.FuncText = p.name;
                newpse.Retest();
                if(TestPatternAny((short)p.id)) {
                    if(selID == -2) {
                        selID = (short)p.id;
                    } else if(selID != -1 && selID != p.id) {
                        selID = -1;
                    }
                }
            }
            foreach(Pattern p in LightDict.inst.warnPatts) {
                newbie = GameObject.Instantiate<GameObject>(prefab);
                newbie.transform.SetParent(menu, false);
                newbie.transform.localScale = Vector3.one;
                newpse = newbie.GetComponent<PattSelectElement>();
                newpse.selID = (short)p.id;
                newpse.ps = this;
                newpse.FuncText = p.name;
                newpse.Retest();
                if(TestPatternAny((short)p.id)) {
                    if(selID == -2) {
                        selID = (short)p.id;
                    } else if(selID != -1 && selID != p.id) {
                        selID = -1;
                    }
                }
            }
            if(selID == -2) {
                enableButton.text = "Cannot Enable, No Pattern";
                enableButton.GetComponent<Button>().interactable = false;

                PhaseA.GetComponent<Button>().interactable = false;
                PhaseB.GetComponent<Button>().interactable = false;
            } else if(selID == -1) {
                enableButton.text = "Enable: <i>Multiple Patterns</i>";
                enableButton.GetComponent<Button>().interactable = true;

                PhaseA.GetComponent<Button>().interactable = false;
                PhaseB.GetComponent<Button>().interactable = false;
            } else {
                Pattern a = null;
                foreach(Pattern p in LightDict.inst.flashPatts) {
                    if(p.id == selID) {
                        a = p;

                        PhaseA.GetComponent<Button>().interactable = true;
                        PhaseB.GetComponent<Button>().interactable = true;

                        break;
                    }
                }
                if(a == null)
                    foreach(Pattern p in LightDict.inst.warnPatts) {
                        if(p.id == selID) {
                            a = p;

                            PhaseA.GetComponent<Button>().interactable = false;
                            PhaseB.GetComponent<Button>().interactable = false;

                            break;
                        }
                    }
                if(a == null) {
                    enableButton.text = "Cannot Enable, No Pattern";
                    enableButton.GetComponent<Button>().interactable = false;
                } else {
                    enableButton.text = "Enable: " + a.name;
                    enableButton.GetComponent<Button>().interactable = true;
                }
            }
        }
    }

    public void RelabelEnable() {
        if(LightDict.inst.steadyBurn.Contains(f) || f == AdvFunction.DIM) {
            enableButton.text = "Enable " + (f == AdvFunction.DIM ? "Light Dimming" : "Steady Burn");
            enableButton.GetComponent<Button>().interactable = true;
        //} else if(f == AdvFunction.TRAFFIC) {
        //    NbtCompound patts = FindObjectOfType<BarManager>().patts.Get<NbtCompound>("traf").Get<NbtCompound>("patt");
        //    short selID = patts["l"].ShortValue;
        //    if(selID != patts["r"].ShortValue) {
        //        selID = -2;
        //    }
        //    if(selID != patts["c"].ShortValue) {
        //        selID = -2;
        //    }

        //    if(selID == -2) {
        //        enableButton.text = "Enable: <i>Multiple Patterns</i>";
        //        enableButton.GetComponent<Button>().interactable = true;
        //    }
        //    if(selID == -1) {
        //        enableButton.text = "Cannot Enable, No Pattern";
        //        enableButton.GetComponent<Button>().interactable = false;
        //    } else {
        //        Pattern a = null;
        //        foreach(Pattern p in LightDict.inst.tdPatts) {
        //            if(p.id == selID) {
        //                a = p;
        //                break;
        //            }
        //        }
        //        if(a == null) {
        //            enableButton.text = "Cannot Enable, No Pattern";
        //            enableButton.GetComponent<Button>().interactable = false;
        //        } else {
        //            enableButton.text = "Enable: " + a.name;
        //            enableButton.GetComponent<Button>().interactable = true;
        //        }
        //    }
        } else {
            bool foundOne = false;
            Pattern a = null;
            foreach(Pattern p in LightDict.inst.flashPatts) {
                if(TestPatternAny((short)p.id)) {
                    if(!foundOne) {
                        foundOne = true;
                        a = p;
                    } else {
                        a = null;
                    }
                }
            }
            foreach(Pattern p in LightDict.inst.warnPatts) {
                if(TestPatternAny((short)p.id)) {
                    if(!foundOne) {
                        foundOne = true;
                        a = p;
                    } else {
                        a = null;
                    }
                }
            }
            if(!foundOne) {
                enableButton.text = "Cannot Enable, No Pattern";
                enableButton.GetComponent<Button>().interactable = false;
            } else if(a == null) {
                enableButton.text = "Enable: <i>Multiple Patterns</i>";
                enableButton.GetComponent<Button>().interactable = true;
            } else {
                enableButton.text = "Enable: " + a.name;
                enableButton.GetComponent<Button>().interactable = true;
            }
        }
    }

    public bool TestPatternAny(short id) {
        return TestPatternAny(id, this.f);
    }

    public bool TestPatternAny(short id, AdvFunction f) {
        NbtCompound patts = FindObjectOfType<BarManager>().patts;
        foreach(LightBlock lb in FindObjectsOfType<LightBlock>()) {
            if(!lb.gameObject.activeInHierarchy || !lb.Selected) continue;
            LightHead lh = null;
            for(Transform t = lb.transform; lh == null && t != null; t = t.parent) {
                lh = t.GetComponent<LightHead>();
            }
            if(lh == null) {
                Debug.LogError("lolnope - " + lb.GetPath() + " can't find a LightHead.", lb);
                ErrorText.inst.DispError(lb.GetPath() + " can't find a LightHead.");
                continue;
            }

            //if(f == AdvFunction.TRAFFIC) {
            //    NbtCompound patCmpd = patts.Get<NbtCompound>("traf").Get<NbtCompound>("patt");
            //    if(patCmpd["left"].ShortValue == id) return true;
            //    if(patCmpd["rite"].ShortValue == id) return true;
            //    if(patCmpd["cntr"].ShortValue == id) return true;
            //} else {
            //    string cmpdName = BarManager.GetFnString(lb.transform, f);
            //    if(cmpdName == null) {
            //        Debug.LogWarning("lolnope - " + f.ToString() + " has no similar setting in the data bytes.  Ask James.");
            //        ErrorText.inst.DispError(f.ToString() + " has no similar setting in the data bytes.  Ask James.");
            //        return false;
            //    }
            //    NbtCompound patCmpd = patts.Get<NbtCompound>(cmpdName).Get<NbtCompound>("pat1");

            //    string tagname = lb.transform.position.z < 0 ? "r" : "f";
            //    string path = lh.transform.GetPath();

            //    if(path.Contains("Corner")) {
            //        tagname = tagname + "cor";
            //    } else if(path.Contains("Inboard")) {
            //        tagname = tagname + "inb";
            //    } else if(path.Contains("Outboard")) {
            //        if(lh.loc == Location.FAR_REAR)
            //            tagname = tagname + "far";
            //        else
            //            tagname = tagname + "oub";
            //    } else if(path.Contains("MidSection")) {
            //        tagname = tagname + "cen";
            //    }

            //    if(patCmpd.Get<NbtShort>(tagname).ShortValue == id) {
            //        return true;
            //    }

            //    patCmpd = patts.Get<NbtCompound>(cmpdName).Get<NbtCompound>("pat2");

            //    if(patCmpd.Get<NbtShort>(tagname).ShortValue == id) {
            //        return true;
            //    }
            //}
        }
        return false;
    }

    public bool TestPatternAll(short id) {
        return TestPatternAll(id, this.f);
    }

    public bool TestPatternAll(short id, AdvFunction f) {
        NbtCompound patts = FindObjectOfType<BarManager>().patts;
        foreach(LightBlock lb in FindObjectsOfType<LightBlock>()) {
            if(!lb.gameObject.activeInHierarchy || !lb.Selected) continue;
            LightHead lh = null;
            for(Transform t = lb.transform; lh == null && t != null; t = t.parent) {
                lh = t.GetComponent<LightHead>();
            }
            if(lh == null) {
                Debug.LogError("lolnope - " + lb.GetPath() + " can't find a LightHead.", lb);
                ErrorText.inst.DispError(lb.GetPath() + " can't find a LightHead.");
                continue;
            }

            //if(f == AdvFunction.TRAFFIC) {
            //    NbtCompound patCmpd = patts.Get<NbtCompound>("traf").Get<NbtCompound>("patt");
            //    if(patCmpd["left"].ShortValue != id) return false;
            //    if(patCmpd["rite"].ShortValue != id) return false;
            //    if(patCmpd["cntr"].ShortValue != id) return false;
            //} else {
            //    string cmpdName = BarManager.GetFnString(lb.transform, f);
            //    if(cmpdName == null) {
            //        Debug.LogWarning("lolnope - " + f.ToString() + " has no similar setting in the data bytes.  Ask James.");
            //        return false;
            //    }
            //    NbtCompound patCmpd = patts.Get<NbtCompound>(cmpdName).Get<NbtCompound>("pat1");

            //    string tagname = lb.transform.position.z < 0 ? "r" : "f";
            //    string path = lh.transform.GetPath();

            //    if(path.Contains("Corner")) {
            //        tagname = tagname + "cor";
            //    } else if(path.Contains("Inboard")) {
            //        tagname = tagname + "inb";
            //    } else if(path.Contains("Outboard")) {
            //        if(lh.loc == Location.FAR_REAR)
            //            tagname = tagname + "far";
            //        else
            //            tagname = tagname + "oub";
            //    } else if(path.Contains("MidSection")) {
            //        tagname = tagname + "cen";
            //    }

            //    if(patCmpd.Get<NbtShort>(tagname).ShortValue != id) {
            //        return false;
            //    }
            //    patCmpd = patts.Get<NbtCompound>(cmpdName).Get<NbtCompound>("pat2");
            //    if(patCmpd.Get<NbtShort>(tagname).ShortValue != id) {
            //        return false;
            //    }
            //}
        }
        return true;
    }
}
