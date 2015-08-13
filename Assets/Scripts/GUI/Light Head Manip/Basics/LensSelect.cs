using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UI Component.  Used to allow the user to select the Lens they want from a list.
/// </summary>
public class LensSelect : MonoBehaviour {
    /// <summary>
    /// The Transform that acts as the parent to the options.  Set via Unity Inspector.
    /// </summary>
    public RectTransform menu;
    /// <summary>
    /// The prefab that we instantiate instances from for each option.  Set via Unity Inspector.
    /// </summary>
    public GameObject optionPrefab;
    /// <summary>
    /// A reference to the camera so we can set the lenses.  Set via Unity Inspector.
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
    }

    /// <summary>
    /// Refreshes this Component, recreating the list of lens options
    /// </summary>
    public void Refresh() {
        Clear();

        foreach(Lens alpha in LightDict.inst.lenses) {
            GameObject newbie = GameObject.Instantiate(optionPrefab) as GameObject;
            newbie.transform.SetParent(menu, false);
            newbie.transform.localScale = Vector3.one;
            newbie.GetComponent<LightOptionElement>().lens = alpha;
            newbie.GetComponent<LightOptionElement>().lensSel = this;
        }

        LayoutRebuilder.MarkLayoutForRebuild(menu);
    }

    /// <summary>
    /// Sets the selected lens on all selected BarSegments.
    /// </summary>
    public void SetSelection(Lens l) {
        foreach(BarSegment seg in cam.SelectedLens) {
            if(seg.Visible) {
                seg.lens = l;
            }
        }

        BarManager.moddedBar = true;
    }
}
