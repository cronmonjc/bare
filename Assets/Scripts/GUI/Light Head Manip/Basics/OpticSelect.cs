using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UI Component.  Used to allow the user to select the Optic they want from a list.
/// </summary>
public class OpticSelect : MonoBehaviour {
    /// <summary>
    /// The next stage.  Set via Unity Inspector.
    /// </summary>
    public StyleSelect styleSelect;
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
        styleSelect.Clear();
        styleSelect.gameObject.SetActive(false);
    }

    /// <summary>
    /// Refreshes this Component.  Clears the listing and creates new buttons for every selectable optic.
    /// </summary>
    public void Refresh() {
        Clear();

        #region Setup
        List<Location> locs = new List<Location>();
        bool showLong = true, showShort = true;
        bool showDual = true, showSingle = true;
        bool showBO = true, showEmi = true;
        List<LightHead> selected = new List<LightHead>(); 
        #endregion
        #region Figure out what optics we can show
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected) {
                selected.Add(alpha);
                alpha.TestSingleDual();
                showLong &= !alpha.isSmall;
                showShort &= alpha.isSmall;
                showDual &= alpha.useDual;
                showSingle &= alpha.useSingle;
                showBO &= alpha.lhd.funcs.Contains(BasicFunction.BLOCK_OFF);
                showEmi &= alpha.lhd.funcs.Contains(BasicFunction.EMITTER);

                if(locs.Contains(alpha.loc)) {
                    continue;
                } else {
                    locs.Add(alpha.loc);
                }
            }
        } 
        #endregion
        if((!showSingle && !showDual)) {
            return;
        }
        LocationNode ln = LightDict.inst.FetchLocation(locs.ToArray()); // Get a Location Node

        if(ln != null) {
            string[] keysArray = new List<string>(ln.optics.Keys).ToArray();
            #region If a head has the Block Off function, show the Block Off "optic"
            if(showBO) {
                GameObject newbie = GameObject.Instantiate(optionPrefab) as GameObject;
                newbie.transform.SetParent(menu, false);
                newbie.transform.localScale = Vector3.one;
				newbie.GetComponent<LightOptionElement>().optNode = ln.optics["dont use"];
                newbie.GetComponent<LightOptionElement>().optSel = this;
            } 
            #endregion

            #region If a head has the Emitter function, show the Emitter optic
            if(showEmi) {
                GameObject newbie = GameObject.Instantiate(optionPrefab) as GameObject;
                newbie.transform.SetParent(menu, false);
                newbie.transform.localScale = Vector3.one;
                newbie.GetComponent<LightOptionElement>().optNode = ln.optics["Emitter"];
                newbie.GetComponent<LightOptionElement>().optSel = this;
            } 
            #endregion
            if(!showBO && !showEmi) {
                for(int i = 0; i < keysArray.Length; i++) {
					if(keysArray[i] == "dont use" || keysArray[i] == "Emitter") {
                        continue;
                    }
                    if(ln.optics[keysArray[i]].dual && !showDual) {
                        continue;
                    } else if(!ln.optics[keysArray[i]].dual && !showSingle) {
                        continue;
                    }

                    if(showLong && !ln.optics[keysArray[i]].fitsLg) {
                        continue;
                    }
                    if(showShort && !ln.optics[keysArray[i]].fitsSm) {
                        continue;
                    }

                    #region Create the different options
                    GameObject newbie = GameObject.Instantiate(optionPrefab) as GameObject;
                    newbie.transform.SetParent(menu, false);
                    newbie.transform.localScale = Vector3.one;
                    newbie.GetComponent<LightOptionElement>().optNode = ln.optics[keysArray[i]];
                    newbie.GetComponent<LightOptionElement>().optSel = this; 
                    #endregion
                }
            }
        }

        #region Create the "empty slot" "optic" option
        GameObject nohead = GameObject.Instantiate(optionPrefab) as GameObject;
        nohead.transform.SetParent(menu, false);
        nohead.transform.localScale = Vector3.one;
        nohead.GetComponent<LightOptionElement>().optNode = null;
        nohead.GetComponent<LightOptionElement>().optSel = this; 
        #endregion

        #region Create notification that dual-color heads can't be used if that's the case
        if (!showDual) {
            for (byte i = 0; i < selected.Count; i++) {
                if (selected[i].isRear && (selected[i].Bit == 0 || selected[i].Bit == 1 || selected[i].Bit == 10 || selected[i].Bit == 11)) {
                    GameObject go = new GameObject("Spacer");
                    go.AddComponent<LayoutElement>().preferredHeight = 10f;
                    go.transform.SetParent(menu, false);
                    go.transform.localScale = Vector3.one;
                    go = new GameObject("Warning");
                    Text goText = go.AddComponent<Text>();
                    goText.text = "There aren't enough dual-color outputs to give all of the selected head(s) a dual-color head.  Please reconfigure your bar to free up the necessary outputs if you need dual-color here.";
                    goText.font = transform.FindChild("Label").GetComponent<Text>().font;
                    goText.color = Color.black;
                    goText.alignment = TextAnchor.UpperCenter;
                    go.transform.SetParent(menu, false);
                    go.transform.localScale = Vector3.one;
                    break;
                }
            }
        }
        #endregion

        #region Figure out which optic is used by all heads (or equivalents)
        OpticNode on = null;
        for(int i = 0; i < selected.Count; i++) {
            if(i == 0) {
                on = selected[i].lhd.optic;
            } else if(on != selected[i].lhd.optic) {
                if(on != null && selected[i].lhd.optic != null && (on.name == selected[i].lhd.optic.smEquivalent || on.name == selected[i].lhd.optic.lgEquivalent)) continue;
                else {
                    on = null;
                    break;
                }
            }
        } 
        #endregion
        #region Show Style Select if there is a shared optic
        styleSelect.selectedType = on;
        if(styleSelect.selectedType != null) {
            styleSelect.Refresh();
            styleSelect.gameObject.SetActive(true);
        } 
        #endregion

        LayoutRebuilder.MarkLayoutForRebuild(menu);
    }

    /// <summary>
    /// Applies the optic selection to all of the selected heads.
    /// </summary>
    /// <param name="node">The Optic to apply.</param>
    public void SetSelection(OpticNode node) {
        #region Show Style Select if there was a selected optic
        styleSelect.selectedType = node;
        styleSelect.gameObject.SetActive(node != null); 
        #endregion
        bool change = false, blank = false;
        foreach(LightHead lh in cam.SelectedHead) {
            if(lh.gameObject.activeInHierarchy && lh.lhd.optic != node) {
                if(node == null) {
                    #region Clear out heads entirely if "empty slot" was selected
                    lh.SetOptic("");
                    lh.lhd.funcs.Clear();
                    blank = true; 
                    #endregion
                } else if(node.fitsSm && !lh.isSmall) {
                    #region Apply large equivalent because head is large but optic is not
                    if(node.lgEquivalent.Length > 0) {
                        lh.SetOptic(node.lgEquivalent);
                        change = true;
                    } 
                    #endregion
                } else if(node.fitsLg && lh.isSmall) {
                    #region Apply small equivalent because head is small but optic is not
                    if(node.smEquivalent.Length > 0) {
                        lh.SetOptic(node.smEquivalent);
                        change = true;
                    } 
                    #endregion
                } else {
                    lh.SetOptic(node); // Head fits, apply optic directly
                    change = true;
                }
            }
        }
        #region Refresh Function Select if "empty slot" was selected.
        if(blank)
            FindObjectOfType<FunctionSelect>().Refresh(); 
        #endregion
        #region Refresh Style Select if an optic was selected
        if(change)
            styleSelect.Refresh(); 
        #endregion
        #region Refresh Basic Phases in case that changed anything
        foreach(BasicPhase alpha in FindObjectsOfType<BasicPhase>()) {
            alpha.Refresh();
        }
        #endregion
    }
}