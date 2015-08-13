using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UI Component.  Used to allow the user to select the Function they want from a list.
/// </summary>
public class FunctionSelect : MonoBehaviour {
    /// <summary>
    /// The next stage.  Set via Unity Inspector.
    /// </summary>
    public OpticSelect opticSelect;
    /// <summary>
    /// The Transform that acts as the parent to the options.  Set via Unity Inspector.
    /// </summary>
    public RectTransform menu;
    /// <summary>
    /// The prefab that we instantiate instances from for each option.  Set via Unity Inspector.
    /// </summary>
    public GameObject optionPrefab;
    /// <summary>
    /// A reference to the camera so we can set the lights.  Set via Unity Inspector.
    /// </summary>
    public CameraControl cam;

    /// <summary>
    /// The list of selected functions
    /// </summary>
    public List<BasicFunction> selFuncs;
    /// <summary>
    /// The list of selectable functions
    /// </summary>
    public List<BasicFunction> potential;

    /// <summary>
    /// Clears this Component.
    /// </summary>
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

    /// <summary>
    /// Refreshes this Component.  Clears the listing and creates new buttons for every selectable function.
    /// </summary>
    public void Refresh() {
        Clear();

        #region Setup
        if(selFuncs == null) selFuncs = new List<BasicFunction>();
        else selFuncs.Clear();
        if(potential == null) potential = new List<BasicFunction>();
        else potential.Clear(); 
        #endregion

        #region Get all of the potential selections
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected) {
                foreach(BasicFunction fn in alpha.CapableBasicFunctions) {
                    if(!potential.Contains(fn)) {
                        potential.Add(fn);
                    }
                }
            }
        } 
        #endregion

        #region Create all of the option GameObjects
        for(int i = 0; i < potential.Count; i++) {
            if(potential[i] == BasicFunction.CRUISE) continue;  // Do Cruise last
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
        #endregion

        #region Create Cruise separately, last
        if(potential.Contains(BasicFunction.CRUISE)) {
            GameObject newbie = GameObject.Instantiate(optionPrefab) as GameObject;
            newbie.transform.SetParent(menu, false);
            newbie.transform.localScale = Vector3.one;
            newbie.GetComponent<LightOptionElement>().fn = BasicFunction.CRUISE;
            newbie.GetComponent<LightOptionElement>().funcSel = this;
        } 
        #endregion

        #region Figure out what functions all of the heads have
        selFuncs.AddRange(potential);

        foreach(LightHead alpha in cam.SelectedHead) {
            foreach(BasicFunction f in potential) {
                if(selFuncs.Contains(f) && !alpha.lhd.funcs.Contains(f)) {
                    selFuncs.Remove(f);
                }
            }
        } 
        #endregion
        #region Show Optic Select if there's more than one selected function
        if(selFuncs.Count > 0) {
            opticSelect.gameObject.SetActive(true);
            opticSelect.Refresh();
        } else {
            opticSelect.gameObject.SetActive(false);
            opticSelect.styleSelect.gameObject.SetActive(false);
        } 
        #endregion

        LayoutRebuilder.MarkLayoutForRebuild(menu);
    }

    /// <summary>
    /// Adds the selected function to all selected heads.
    /// </summary>
    /// <param name="fn">The function that was selected.</param>
    public void SetSelection(BasicFunction fn) {
        #region Figure out if we need to add or remove the function
        bool add = false;
        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(lh.gameObject.activeInHierarchy && lh.Selected) {
                if(!lh.lhd.funcs.Contains(fn)) add = true;
            }
        } 
        #endregion

        #region Actually add/remove function from heads
        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(lh.gameObject.activeInHierarchy && lh.Selected) {
                if(add) {
                    lh.AddBasicFunction(fn);
                } else {
                    lh.RemoveBasicFunction(fn);
                }
            }
        } 
        #endregion

        #region Figure out what functions all of the heads have
        selFuncs.Clear();
        selFuncs.AddRange(potential);
        foreach(LightHead alpha in cam.SelectedHead) {
            foreach(BasicFunction f in potential) {
                if(selFuncs.Contains(f) && !alpha.lhd.funcs.Contains(f)) {
                    selFuncs.Remove(f); // If any singular head doesn't have a function, remove from selFuncs
                }
            }
        } 
        #endregion
        #region Show Optic Select if there's more than one selected function
        if(selFuncs.Count > 0) {
            opticSelect.gameObject.SetActive(true);
            opticSelect.Refresh();
        } else {
            opticSelect.gameObject.SetActive(false);
            opticSelect.styleSelect.gameObject.SetActive(false);
        } 
        #endregion

        FindObjectOfType<AdvPattDisp>().Refresh();  // Refresh AdvPattDisp, showing Advanced Functions to user on far right pane
        foreach(BasicPhase alpha in FindObjectsOfType<BasicPhase>()) {
            alpha.Refresh(); // Refresh all BasicPhase buttons
        }
    }
}