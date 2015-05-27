using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// GUI Item.  Used to allow the user to select the Style they want from a list.
/// </summary>
public class StyleSelect : MonoBehaviour {
    /// <summary>
    /// OpticNode to define what the user is allowed to select from.
    /// </summary>
    public OpticNode selectedType;
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

    private bool IsRecommended(StyleNode sn) {
        bool rtn = true;

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected) {
                foreach(BasicFunction f in alpha.lhd.funcs) {
                    switch(f) {
                        case BasicFunction.ALLEY:
                        case BasicFunction.TAKEDOWN:
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

                //foreach(AdvFunction f in alpha.patterns.Keys) {
                //    switch(f) {
                //        case AdvFunction.TAKEDOWN:
                //        case AdvFunction.ICL:
                //        case AdvFunction.ALLEY:
                //            rtn &= sn.partSuffix.Contains("C");
                //            break;
                //        case AdvFunction.T13:
                //        case AdvFunction.STT_AND_TAIL:
                //            rtn &= sn.partSuffix.Contains("R");
                //            break;
                //        case AdvFunction.TRAFFIC:
                //            rtn &= sn.partSuffix.Contains("A");
                //            break;
                //        default: break;
                //    }
                //}
            }
        }

        return rtn;
    }

    void Update() {
        if(selectedType == null) {
            gameObject.SetActive(false);
            return;
        }
    }

    public void SetSelection(StyleNode node) {
        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(lh.gameObject.activeInHierarchy && lh.Selected)
                lh.SetStyle(node);
        }
    }
}