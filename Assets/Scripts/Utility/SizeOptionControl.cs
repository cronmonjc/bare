using UnityEngine;
using System.Collections;

/// <summary>
/// Component that controls the displayed size of the LightHeads by showing/hiding short and long versions of them
/// </summary>
public class SizeOptionControl : MonoBehaviour {

    /// <summary>
    /// The reference to the long LightHead GameObject
    /// </summary>
    private GameObject longGO;
    /// <summary>
    /// The reference to the short LightHead GameObject
    /// </summary>
    private GameObject shortGO;

    /// <summary>
    /// Can this Component show the long GameObject on a given size?  Set via Unity Inspector.
    /// </summary>
    public bool[] canLong = new bool[] { true, true, true, true, true };
    /// <summary>
    /// Can this Component show the short GameObject on a given size?  Set via Unity Inspector.
    /// </summary>
    public bool[] canShort = new bool[] { true, true, true, true, true };

    /// <summary>
    /// Gets or sets a value indicating whether it should show the longer version of the LightHeads.
    /// </summary>
    public bool ShowLong {
        get {
            return longGO.activeInHierarchy;
        }
        set {
            int size = BarManager.inst.BarSize; // Find the size of the light bar

            #region Find the currently selected LightHead for later reference
            LightHead[] lhs = transform.GetComponentsInChildren<LightHead>(true);
            LightHead lh = null;
            foreach(LightHead alpha in lhs) {
                if(alpha.Selected && ((value && alpha.isSmall) || (!value && !alpha.isSmall))) {
                    lh = alpha;
                    break;
                }
            } 
            #endregion

            #region Show the proper GameObject, based on what's available
            if(canLong[size] && !canShort[size]) {
                longGO.SetActive(true);
                shortGO.SetActive(false);
            } else if(!canLong[size] && canShort[size]) {
                longGO.SetActive(false);
                shortGO.SetActive(true);
            } else {
                longGO.SetActive(value && canLong[size]);
                shortGO.SetActive(!value && canShort[size]);
            } 
            #endregion
            if(lh != null) {
                foreach(LightHead alpha in lhs) {
                    if(!alpha.Selected) {
                        if(lh.lhd.optic != null && lh.lhd.style != null) {
                            #region Clear any traffic if resizing any traffic heads
                            if(alpha.lhd.funcs.Contains(BasicFunction.TRAFFIC) && alpha.shouldBeTD) {
                                BarManager.inst.td = TDOption.NONE;
                                foreach(LightHead beta in BarManager.inst.allHeads) {
                                    beta.shouldBeTD = false;
                                }
                            } 
                            #endregion
                            #region Copy functions over
                            alpha.lhd.funcs.Clear();
                            foreach(BasicFunction f in lh.lhd.funcs) {
                                alpha.AddBasicFunction(f, false);
                            }
                            alpha.RefreshBasicFuncDefault(); 
                            #endregion

                            #region Copy optics and styles over when possible
                            if(alpha.lhd.optic.styles.ContainsKey(lh.lhd.style.name)) {
                                if(alpha.lhd.optic.styles[lh.lhd.style.name].selectable)
                                    alpha.SetStyle(lh.lhd.style);
                            } else {
                                alpha.SetOptic(value ? lh.lhd.optic.lgEquivalent : lh.lhd.optic.smEquivalent, doDefault: false);
                                alpha.SetStyle(lh.lhd.style.name);
                            } 
                            #endregion
                        }
                    }
                }
                #region Select any heads that were shown from the apparent splitting/merging
                foreach(LightHead alpha in lhs) {
                    alpha.Selected = alpha.isSmall ^ value;
                } 
                #endregion
            }
        }
    }


    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        longGO = transform.FindChild("L").gameObject; // Finds short and long versions
        shortGO = transform.FindChild("DS").gameObject;

        ShowLong = true; // Show long right off the bat, hide shorts
    }

}
