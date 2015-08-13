using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UI Component.  Used to allow the user to select the Style they want from a list.
/// </summary>
public class StyleSelect : MonoBehaviour {
    /// <summary>
    /// OpticNode to define what the user is allowed to select from.  Set via Unity Inspector.
    /// </summary>
    public OpticNode selectedType;
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
    }

    /// <summary>
    /// Refreshes this Component, relisting all of the available styles.
    /// </summary>
    public void Refresh() {
        Clear();

        if(selectedType != null) {
            string[] keysArray = new List<string>(selectedType.styles.Keys).ToArray();
            for(int i = 0; i < keysArray.Length; i++) {
                GameObject newbie = GameObject.Instantiate(optionPrefab) as GameObject;
                newbie.transform.SetParent(menu, false);
                newbie.transform.localScale = Vector3.one;
                StyleNode sn = selectedType.styles[keysArray[i]];
                newbie.GetComponent<LightOptionElement>().styNode = sn;
                newbie.GetComponent<LightOptionElement>().stySel = this;
                newbie.GetComponent<LightOptionElement>().recommended = IsRecommended(sn);
            }

            LayoutRebuilder.MarkLayoutForRebuild(menu);
        }
    }

    /// <summary>
    /// Determines whether the specified style is recommended for the light head.
    /// </summary>
    /// <param name="lh">The light head to test against.</param>
    /// <param name="sn">The style to test.</param>
    /// <returns>True if the style is recommended.</returns>
    public static bool IsRecommended(LightHead lh, StyleNode sn) {
        bool rtn = true;

        foreach(BasicFunction f in lh.lhd.funcs) {
            switch(f) {
                case BasicFunction.STEADY:
                    rtn &= sn.partSuffix.Contains("C");
                    break;
                case BasicFunction.CAL_STEADY:
                case BasicFunction.STT:
                    rtn &= sn.partSuffix.Contains("R");
                    break;
                case BasicFunction.TRAFFIC:
                    rtn &= sn.partSuffix.Contains("A");
                    break;
                default:
                    break;
            }
        }

        return rtn;
    }

    /// <summary>
    /// Determines whether the specified style node is recommended for all selected heads.
    /// </summary>
    /// <param name="sn">The style node to test.</param>
    /// <returns>True if the style is recommended.</returns>
    public static bool IsRecommended(StyleNode sn) {
        bool rtn = true;

        if(sn.name.Contains("Logo")) { // Block Off's "No Logo" and "Logo"
            return true;
        }

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected) {
                rtn &= IsRecommended(alpha, sn);
            }
        }

        return rtn;
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(selectedType == null) {
            gameObject.SetActive(false);
            return;
        }
    }

    /// <summary>
    /// Sets the styles of the selected heads.
    /// </summary>
    /// <param name="node">The style to apply.</param>
    public void SetSelection(StyleNode node) {
        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(lh.gameObject.activeInHierarchy && lh.Selected)
                lh.SetStyle(node);
        }
        foreach(BasicPhase alpha in FindObjectsOfType<BasicPhase>()) {
            alpha.Refresh();
        }
        FindObjectOfType<AdvPattDisp>().Refresh();
        FindObjectOfType<AltNumberingHide>().Refresh();
    }
}