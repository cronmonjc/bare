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
        potential.Add(BasicFunction.FLASH_STEADY);
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected) {
                switch(alpha.loc) {
                    case Location.FRONT:
                        if(potential.Contains(BasicFunction.EMITTER)) continue;
                        potential.Add(BasicFunction.EMITTER);
                        potential.Add(BasicFunction.CAL_STEADY);
                        continue;
                    case Location.REAR:
                        if(potential.Contains(BasicFunction.TRAFFIC)) continue;
                        potential.Add(BasicFunction.TRAFFIC);
                        continue;
                    case Location.FAR_REAR:
                        if(potential.Contains(BasicFunction.STT)) continue;
                        potential.Add(BasicFunction.STT);
                        continue;
                    case Location.FRONT_CORNER:
                    case Location.REAR_CORNER:
                        if(potential.Contains(BasicFunction.CRUISE)) continue;
                        potential.Add(BasicFunction.CRUISE);
                        continue;
                }
            }
        }

        for(int i = 0; i < potential.Count; i++) {
            GameObject newbie = GameObject.Instantiate(optionPrefab) as GameObject;
            newbie.transform.SetParent(menu, false);
            newbie.transform.localScale = Vector3.one;
            newbie.GetComponent<LightOptionElement>().fn = potential[i];
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
        bool add = !opticSelect.fn.Contains(fn);
        if(add)
            opticSelect.fn.Add(fn);
        else
            opticSelect.fn.Remove(fn);

        opticSelect.gameObject.SetActive(opticSelect.fn.Count > 0);
        bool change = false;
        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(lh.gameObject.activeInHierarchy && lh.Selected) {
                if(add && !lh.lhd.funcs.Contains(fn)) {
                    List<BasicFunction> potential = new List<BasicFunction>();
                    potential.Add(BasicFunction.FLASHING);
                    potential.Add(BasicFunction.FLASH_STEADY);
                    switch(lh.loc) {
                        case Location.FRONT:
                            potential.Add(BasicFunction.EMITTER);
                            potential.Add(BasicFunction.CAL_STEADY);
                            break;
                        case Location.REAR:
                            potential.Add(BasicFunction.TRAFFIC);
                            break;
                        case Location.FAR_REAR:
                            potential.Add(BasicFunction.STT);
                            break;
                        case Location.FRONT_CORNER:
                        case Location.REAR_CORNER:
                            potential.Add(BasicFunction.CRUISE);
                            break;
                    }
                    if(potential.Contains(fn))
                        lh.lhd.funcs.Add(fn);
                    change = true;
                } else if(lh.lhd.funcs.Contains(fn)) {
                    lh.lhd.funcs.Remove(fn);
                    change = true;
                }
            }
        }
        while(opticSelect.fn.Count > 2) {
            fn = opticSelect.fn[0];
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
            foreach(LightHead lh in cam.OnlyCamSelected) {
                lh.SetOptic("");
            }
            opticSelect.Refresh();
        }
    }
}