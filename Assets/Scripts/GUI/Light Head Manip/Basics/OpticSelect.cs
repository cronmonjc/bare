using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// GUI Item.  Used to allow the user to select the Type they want from a list.
/// </summary>
public class OpticSelect : MonoBehaviour {
    /// <summary>
    /// The next stage.
    /// </summary>
    public StyleSelect styleSelect;
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
            Destroy(alpha.gameObject);
        }
        styleSelect.Clear();
        styleSelect.gameObject.SetActive(false);
    }

    public void Refresh() {
        Clear();

        List<Location> locs = new List<Location>();
        bool showLong = true, showShort = true;
        bool showDual = true, showSingle = true;
        bool showBO = true, showEmi = true;
        List<LightHead> selected = new List<LightHead>();
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected) {
                selected.Add(alpha);
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
        if((!showSingle && !showDual)) {
            return;
        }
        LocationNode ln = LightDict.inst.FetchLocation(locs.ToArray());

        if(ln != null) {
            string[] keysArray = new List<string>(ln.optics.Keys).ToArray();
            if(showBO) {
                GameObject newbie = GameObject.Instantiate(optionPrefab) as GameObject;
                newbie.transform.SetParent(menu, false);
                newbie.transform.localScale = Vector3.one;
                newbie.GetComponent<LightOptionElement>().optNode = ln.optics["Block Off"];
                newbie.GetComponent<LightOptionElement>().optSel = this;
            }

            if(showEmi) {
                GameObject newbie = GameObject.Instantiate(optionPrefab) as GameObject;
                newbie.transform.SetParent(menu, false);
                newbie.transform.localScale = Vector3.one;
                newbie.GetComponent<LightOptionElement>().optNode = ln.optics["Emitter"];
                newbie.GetComponent<LightOptionElement>().optSel = this;
            }
            if(!showBO && !showEmi) {
                for(int i = 0; i < keysArray.Length; i++) {
                    if(keysArray[i] == "Block Off" || keysArray[i] == "Emitter") {
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

                    GameObject newbie = GameObject.Instantiate(optionPrefab) as GameObject;
                    newbie.transform.SetParent(menu, false);
                    newbie.transform.localScale = Vector3.one;
                    newbie.GetComponent<LightOptionElement>().optNode = ln.optics[keysArray[i]];
                    newbie.GetComponent<LightOptionElement>().optSel = this;
                }
            }
        }

        GameObject nohead = GameObject.Instantiate(optionPrefab) as GameObject;
        nohead.transform.SetParent(menu, false);
        nohead.transform.localScale = Vector3.one;
        nohead.GetComponent<LightOptionElement>().optNode = null;
        nohead.GetComponent<LightOptionElement>().optSel = this;

        OpticNode on = null;
        for(int i = 0; i < selected.Count; i++) {
            if(i == 0) {
                on = selected[i].lhd.optic;
            } else if(on != selected[i].lhd.optic) {
                if(on.name == selected[i].lhd.optic.smEquivalent || on.name == selected[i].lhd.optic.lgEquivalent) continue;
                else {
                    on = null;
                    break;
                }
            }
        }
        styleSelect.selectedType = on;
        if(styleSelect.selectedType != null) {
            styleSelect.Refresh();
            styleSelect.gameObject.SetActive(true);
        }

        LayoutRebuilder.MarkLayoutForRebuild(menu);
    }

    public void SetSelection(OpticNode node) {
        styleSelect.selectedType = node;
        styleSelect.gameObject.SetActive(node != null);
        bool change = false, blank = false;
        foreach(LightHead lh in cam.OnlyCamSelected) {
            if(lh.gameObject.activeInHierarchy && lh.lhd.optic != node) {
                if(node == null) {
                    lh.SetOptic("");
                    blank = true;
                } else if(node.fitsSm && !lh.isSmall) {
                    if(node.lgEquivalent.Length > 0) {
                        lh.SetOptic(node.lgEquivalent);
                        change = true;
                    }
                } else if(node.fitsLg && lh.isSmall) {
                    if(node.smEquivalent.Length > 0) {
                        lh.SetOptic(node.smEquivalent);
                        change = true;
                    }
                } else {
                    lh.SetOptic(node);
                    change = true;
                }
            }
        }
        if(blank)
            FindObjectOfType<FunctionSelect>().Refresh();
        if(change)
            styleSelect.Refresh();
        foreach(BasicPhase alpha in FindObjectsOfType<BasicPhase>()) {
            alpha.Refresh();
        }
    }
}