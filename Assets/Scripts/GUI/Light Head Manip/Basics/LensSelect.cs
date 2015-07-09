using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// GUI Item.  Used to allow the user to select the Lens they want from a list.
/// </summary>
public class LensSelect : MonoBehaviour {
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
    }

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

    public void SetSelection(Lens l) {
        foreach(BarSegment seg in cam.SelectedLens) {
            if(seg.Visible) {
                seg.lens = l;
            }
        }

        BarManager.moddedBar = true;
    }
}
