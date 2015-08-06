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

    public List<BasicFunction> selFuncs, potential;

    public void Clear() {
        List<Transform> temp = new List<Transform>();
        foreach(Transform alpha in menu) {
            temp.Add(alpha);
        }
        foreach(Transform alpha in temp) {
            Destroy(alpha.gameObject);
        }
        opticSelect.Clear();
        opticSelect.gameObject.SetActive(false);
    }

    public void Refresh() {
        Clear();

        if(selFuncs == null) selFuncs = new List<BasicFunction>();
        else selFuncs.Clear();
        if(potential == null) potential = new List<BasicFunction>();
        else potential.Clear();

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected) {
                foreach(BasicFunction fn in alpha.CapableBasicFunctions) {
                    if(!potential.Contains(fn)) {
                        potential.Add(fn);
                    }
                }
            }
        }

        for(int i = 0; i < potential.Count; i++) {
            if(potential[i] == BasicFunction.CRUISE) continue;
            if(potential[i] == BasicFunction.EMITTER) {
                if(cam.SelectedHead.Count > 1) continue;
                bool found = false;
                foreach(LightHead alpha in BarManager.inst.allHeads) {
                    if(alpha.gameObject.activeInHierarchy && !alpha.Selected && alpha.lhd.funcs.Contains(BasicFunction.EMITTER)) {
                        found = true;
                        break;
                    }
                }
                if(found) continue;
            }

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

        selFuncs.AddRange(potential);

        foreach(LightHead alpha in cam.SelectedHead) {
            foreach(BasicFunction f in potential) {
                if(selFuncs.Contains(f) && !alpha.lhd.funcs.Contains(f)) {
                    selFuncs.Remove(f);
                }
            }
        }
        if(selFuncs.Count > 0) {
            opticSelect.gameObject.SetActive(true);
            opticSelect.Refresh();
        } else {
            opticSelect.gameObject.SetActive(false);
            opticSelect.styleSelect.gameObject.SetActive(false);
        }

        LayoutRebuilder.MarkLayoutForRebuild(menu);
    }

    public void SetSelection(BasicFunction fn) {
        bool add = false;
        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(lh.gameObject.activeInHierarchy && lh.Selected) {
                if(!lh.lhd.funcs.Contains(fn)) add = true;
            }
        }

        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(lh.gameObject.activeInHierarchy && lh.Selected) {
                if(add) {
                    lh.AddBasicFunction(fn);
                } else {
                    lh.RemoveBasicFunction(fn);
                }
            }
        }

        selFuncs.Clear();
        selFuncs.AddRange(potential);
        foreach(LightHead alpha in cam.SelectedHead) {
            foreach(BasicFunction f in potential) {
                if(selFuncs.Contains(f) && !alpha.lhd.funcs.Contains(f)) {
                    selFuncs.Remove(f);
                }
            }
        }
        if(selFuncs.Count > 0) {
            opticSelect.gameObject.SetActive(true);
            opticSelect.Refresh();
        } else {
            opticSelect.gameObject.SetActive(false);
            opticSelect.styleSelect.gameObject.SetActive(false);
        }

        FindObjectOfType<AdvPattDisp>().Refresh();
        foreach(BasicPhase alpha in FindObjectsOfType<BasicPhase>()) {
            alpha.Refresh();
        }
    }
}