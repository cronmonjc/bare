using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// GUI Item.  Used to allow the user to select the Function they want from a list.
/// </summary>
public class FunctionSelect : MonoBehaviour {
    /// <summary>
    /// The next stage.
    /// </summary>
    public OpticSelect opticSelect;
    /// <summary>
    /// The Transform that acts as the parent to the options.
    /// </summary>
    public RectTransform menu;
    /// <summary>
    /// The prefab that we instantiate instances from for each option.
    /// </summary>
    public GameObject optionPrefab;
    /// <summary>
    /// A reference to the camera so we can set the lights.
    /// </summary>
    public CameraControl cam;

    public void Clear() {
        List<Transform> temp = new List<Transform>();
        foreach(Transform alpha in menu) {
            temp.Add(alpha);
        }
        foreach(Transform alpha in temp) {
            DestroyImmediate(alpha.gameObject);
        }
        opticSelect.Clear();
        opticSelect.gameObject.SetActive(false);
    }

    public void Refresh() {
        Clear();

        List<BasicFunction> potential = new List<BasicFunction>();
        potential.Add(BasicFunction.FLASHING);
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected) {
                switch(alpha.loc) {
                    case Location.ALLEY:
                        if(!potential.Contains(BasicFunction.ALLEY)) potential.Add(BasicFunction.ALLEY);
                        continue;
                    case Location.FRONT:
                        if(!potential.Contains(BasicFunction.TAKEDOWN)) potential.Add(BasicFunction.TAKEDOWN);
                        if(!potential.Contains(BasicFunction.EMITTER) && !alpha.isSmall) potential.Add(BasicFunction.EMITTER);
                        if(!potential.Contains(BasicFunction.CAL_STEADY)) potential.Add(BasicFunction.CAL_STEADY);
                        continue;
                    case Location.REAR:
                        if(!potential.Contains(BasicFunction.TAKEDOWN)) potential.Add(BasicFunction.TAKEDOWN);
                        if(!potential.Contains(BasicFunction.TRAFFIC)) potential.Add(BasicFunction.TRAFFIC);
                        continue;
                    case Location.FAR_REAR:
                        if(!potential.Contains(BasicFunction.TAKEDOWN)) potential.Add(BasicFunction.TAKEDOWN);
                        if(!potential.Contains(BasicFunction.STT)) potential.Add(BasicFunction.STT);
                        continue;
                    case Location.FRONT_CORNER:
                    case Location.REAR_CORNER:
                        if(!potential.Contains(BasicFunction.TAKEDOWN)) potential.Add(BasicFunction.TAKEDOWN);
                        if(!potential.Contains(BasicFunction.CRUISE)) potential.Add(BasicFunction.CRUISE);
                        continue;
                }
            }
        }

        for(int i = 0; i < potential.Count; i++) {
            if(potential[i] == BasicFunction.CRUISE) continue;

            GameObject newbie = GameObject.Instantiate(optionPrefab) as GameObject;
            newbie.transform.SetParent(menu, false);
            newbie.transform.localScale = Vector3.one;
            newbie.GetComponent<LightOptionElement>().fn = potential[i];
            newbie.GetComponent<LightOptionElement>().funcSel = this;
        }

        if(potential.Contains(BasicFunction.CRUISE)) {
            GameObject newbie = GameObject.Instantiate(optionPrefab) as GameObject;
            newbie.transform.SetParent(menu, false);
            newbie.transform.localScale = Vector3.one;
            newbie.GetComponent<LightOptionElement>().fn = BasicFunction.CRUISE;
            newbie.GetComponent<LightOptionElement>().funcSel = this;
        }

        opticSelect.fn.Clear();
        opticSelect.fn.AddRange(potential);
        foreach(LightHead alpha in cam.OnlyCamSelected) {
            if(alpha.lhd.funcs.Count == 0) {
                opticSelect.fn.Clear();
                break;
            }
            foreach(BasicFunction f in new List<BasicFunction>(opticSelect.fn)) {
                if(!alpha.lhd.funcs.Contains(f)) {
                    opticSelect.fn.Remove(f);
                }
            }
        }
        if(opticSelect.fn.Count > 0) {
            opticSelect.Refresh();
            opticSelect.gameObject.SetActive(true);
        }

        LayoutRebuilder.MarkLayoutForRebuild(menu);
    }

    public void SetSelection(BasicFunction fn) {
        bool add = true;
        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(lh.gameObject.activeInHierarchy && lh.Selected) {
                if(lh.lhd.funcs.Contains(fn)) add = false;
            }
        }

        if(add && fn == BasicFunction.EMITTER) {
            opticSelect.fn.Clear();
            opticSelect.fn.Add(fn);
        } else if(add) {
            if(opticSelect.fn.Contains(BasicFunction.EMITTER)) {
                opticSelect.fn.Remove(BasicFunction.EMITTER);
            }
            opticSelect.fn.Add(fn);
        } else
            opticSelect.fn.Remove(fn);

        opticSelect.gameObject.SetActive(opticSelect.fn.Count > 0);
        bool change = false;
        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(lh.gameObject.activeInHierarchy && lh.Selected) {
                if(add && !lh.lhd.funcs.Contains(fn)) {
                    if(fn == BasicFunction.EMITTER) {
                        if(lh.loc == Location.FRONT && !lh.isSmall) {
                            lh.lhd.funcs.Clear();
                            lh.lhd.funcs.Add(fn);
                            lh.SetOptic("Emitter", fn);
                            change = true;
                        }
                    } else {
                        if(lh.lhd.funcs.Contains(BasicFunction.EMITTER)) {
                            lh.lhd.funcs.Remove(BasicFunction.EMITTER);
                        }

                        List<BasicFunction> potential = new List<BasicFunction>();
                        potential.Add(BasicFunction.FLASHING);
                        switch(lh.loc) {
                            case Location.ALLEY:
                                potential.Add(BasicFunction.ALLEY);
                                break;
                            case Location.FRONT:
                                potential.Add(BasicFunction.TAKEDOWN);
                                potential.Add(BasicFunction.CAL_STEADY);
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
                        if(potential.Contains(fn)) {
                            lh.lhd.funcs.Add(fn);

                            if(lh.lhd.funcs.Count == 2) {
                                if(lh.isSmall) {
                                    lh.SetOptic("Dual Small Lineum");
                                } else {
                                    lh.SetOptic("Dual Lineum");
                                }
                            } else if(lh.lhd.funcs.Count == 1) {
                                switch(fn) {
                                    case BasicFunction.TAKEDOWN:
                                    case BasicFunction.ALLEY:
                                    case BasicFunction.STT:
                                    case BasicFunction.TRAFFIC:
                                        if(lh.isSmall) lh.SetOptic("Starburst", fn);
                                        else lh.SetOptic("");
                                        break;
                                    case BasicFunction.FLASHING:
                                    case BasicFunction.CAL_STEADY:
                                        if(lh.isSmall) lh.SetOptic("Small Lineum", fn);
                                        else lh.SetOptic("Lineum", fn);
                                        break;
                                }
                            }
                        }
                        change = true;
                    }
                } else if(!add && lh.lhd.funcs.Contains(fn)) {
                    lh.lhd.funcs.Remove(fn);

                    if(lh.lhd.funcs.Count == 0) {
                        lh.SetOptic("");
                    } else if(lh.lhd.funcs.Count == 1) {
                        switch(lh.lhd.funcs[0]) {
                            case BasicFunction.TAKEDOWN:
                            case BasicFunction.ALLEY:
                            case BasicFunction.STT:
                            case BasicFunction.TRAFFIC:
                                if(lh.isSmall) lh.SetOptic("Starburst", fn);
                                else lh.SetOptic("");
                                break;
                            case BasicFunction.FLASHING:
                            case BasicFunction.CAL_STEADY:
                                if(lh.isSmall) lh.SetOptic("Small Lineum", fn);
                                else lh.SetOptic("Lineum", fn);
                                break;
                        }
                    }
                    change = true;
                }
            }
        }
        while(opticSelect.fn.Count > 2) {
            if(opticSelect.fn.Contains(BasicFunction.CRUISE) && opticSelect.fn.Count == 3) break;

            fn = opticSelect.fn[0];
            if(fn == BasicFunction.CRUISE) {
                fn = opticSelect.fn[1];
            }
            foreach(LightHead lh in BarManager.inst.allHeads) {
                if(lh.gameObject.activeInHierarchy && lh.Selected) {
                    if(lh.lhd.funcs.Contains(fn)) {
                        lh.lhd.funcs.Remove(fn);
                        change = true;
                    }
                }
            }
            opticSelect.fn.RemoveAt(0);
        }

        if(change) {
            opticSelect.Refresh();
        }
    }
}