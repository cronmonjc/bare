using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// GUI Item.  Used to allow the user to select the Type they want from a list.
/// </summary>
// TODO: CLEANUP
public class OpticSelect : MonoBehaviour {
    /// <summary>
    /// The next stage.
    /// </summary>
    public StyleSelect styleSelect;
    /// <summary>
    /// 
    /// </summary>
    public List<BasicFunction> fn;
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
        styleSelect.Clear();
        styleSelect.gameObject.SetActive(false);
    }

    public void Refresh() {
        Clear();

        List<Location> locs = new List<Location>();
        bool showLong = true, showShort = true;
        List<LightHead> selected = new List<LightHead>();
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected) {
                selected.Add(alpha);
                showLong &= !alpha.isSmall;
                showShort &= alpha.isSmall;

                if(locs.Contains(alpha.loc)) {
                    continue;
                } else {
                    locs.Add(alpha.loc);
                }
            }
        }

        LocationNode ln = LightDict.inst.FetchLocation(locs.ToArray());

        if(ln != null) {
            string[] keysArray = new List<string>(ln.optics.Keys).ToArray();
            for(int i = 0; i < keysArray.Length; i++) {
                if(ln.optics[keysArray[i]].dual && fn.Count != 2) {
                    continue;
                } else if(!ln.optics[keysArray[i]].dual && fn.Count != 1) {
                    continue;
                } else if(ln.optics[keysArray[i]].name == "Emitter" ^ (fn.Count == 1 && fn[0] == BasicFunction.EMITTER)) {
                    continue;
                }

                if((showLong && ln.optics[keysArray[i]].fitsLg) || (showShort && ln.optics[keysArray[i]].fitsSm)) {
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
                on = null;
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
        bool change = false;
        foreach(LightHead lh in cam.OnlyCamSelected) {
            if(lh.gameObject.activeInHierarchy && lh.lhd.optic != node) {
                lh.SetOptic(node);
                change = true;
            }
        }
        if(change)
            styleSelect.Refresh();
    }
}