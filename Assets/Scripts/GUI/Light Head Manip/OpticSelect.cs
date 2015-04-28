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
            DestroyImmediate(alpha.gameObject);
        }
    }

    public void Refresh() {
        Clear();
        styleSelect.gameObject.SetActive(false);

        List<Location> locs = new List<Location>();
        bool showLong = true, showShort = true;
        foreach(LightHead alpha in cam.OnlyCamSelected) {
            showLong &= !alpha.isSmall;
            showShort &= alpha.isSmall;

            if(locs.Contains(alpha.loc)) {
                continue;
            } else {
                locs.Add(alpha.loc);
            }
        }

        LocationNode ln = LightDict.inst.FetchLocation(locs.ToArray());

        if(ln != null) {
            string[] keysArray = new List<string>(ln.optics.Keys).ToArray();
            for(int i = 0; i < keysArray.Length; i++) {
                if((showLong && ln.optics[keysArray[i]].fitsLg) || (showShort && ln.optics[keysArray[i]].fitsSm)) {
                    GameObject newbie = GameObject.Instantiate(optionPrefab) as GameObject;
                    newbie.transform.SetParent(menu, false);
                    newbie.transform.localScale = Vector3.one;
                    newbie.GetComponent<LightOptionElement>().optNode = ln.optics[keysArray[i]];
                    newbie.GetComponent<LightOptionElement>().os = this;
                }
            }
        }

        GameObject nohead = GameObject.Instantiate(optionPrefab) as GameObject;
        nohead.transform.SetParent(menu, false);
        nohead.transform.localScale = Vector3.one;
        nohead.GetComponent<LightOptionElement>().optNode = null;
        nohead.GetComponent<LightOptionElement>().os = this;

        OpticNode on = null;
        for(int i = 0; i < cam.OnlyCamSelected.Count; i++) {
            if(i == 0) {
                on = cam.OnlyCamSelected[i].lhd.optic;
            } else if(on != cam.OnlyCamSelected[i].lhd.optic) {
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
        foreach(LightHead lh in new List<LightHead>(cam.OnlyCamSelected)) {
            if(lh.lhd.optic != node) {
                lh.SetOptic(node);
                change = true;
            }
        }
        if(change)
            styleSelect.Refresh();
    }
}