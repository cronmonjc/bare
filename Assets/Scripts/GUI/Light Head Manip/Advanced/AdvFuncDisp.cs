using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;

/// <summary>
/// UI Component.  Used to display what pattern selected head(s) are using.
/// </summary>
public class AdvFuncDisp : MonoBehaviour {
    /// <summary>
    /// A reference to the CameraControl object, to make figuring out selected heads easier.
    /// </summary>
    public static CameraControl cam;
    /// <summary>
    /// Which function this object is supposed to display information for.  Set by the Unity Inspector.
    /// </summary>
    public AdvFunction func;

    /// <summary>
    /// Output Text Component.  Set by the Unity Inspector.
    /// </summary>
    public Text c1patt, c2patt, c1phase, c2phase, dim;
    /// <summary>
    /// Output Image Component (checkmarks indicating enables).  Set by the Unity Inspector.
    /// </summary>
    public Image c1enable, c2enable;

    /// <summary>
    /// Refreshes this Component.
    /// </summary>
    public void Refresh() {
        if(cam == null) { cam = FindObjectOfType<CameraControl>(); }

        #region Setup
        bool c1anyEnabled = false, c1allEnabled = true;
        bool c2anyEnabled = false, c2allEnabled = true;
        string c1ph = "", c2ph = ""; 
        #endregion

        //c2enable.enabled = c1enable.enabled = true;
        //if(c1phase != null) c1phase.text = "";
        //if(c2phase != null) c2phase.text = "";

        foreach(LightHead alpha in cam.SelectedHead) {
            if(!alpha.gameObject.activeInHierarchy) continue;

            byte bit = alpha.Bit;

            bool thisEnabled = false;

            #region Enables
            if(alpha.GetCanEnable(func)) {
                thisEnabled = alpha.GetIsEnabled(func, false, true);
                c1allEnabled &= thisEnabled;
                c1anyEnabled |= thisEnabled;

                thisEnabled = alpha.GetIsEnabled(func, true);
                c2allEnabled &= thisEnabled;
                c2anyEnabled |= thisEnabled;
            } 
            #endregion

            #region Phases
            if(c1phase != null) {
                bool b = alpha.GetPhaseB(func, false);

                if(c1ph == "") {
                    c1ph = "( Phase " + (b ? "B" : "A") + " )";
                } else if((b && c1ph.EndsWith("A )")) || (!b && c1ph.EndsWith("B )"))) {
                    c1ph = "( Mixed Phase )";
                }
            }

            if(c2phase != null) {
                bool b = alpha.GetPhaseB(func, true);

                if(c2ph == "") {
                    c2ph = "( Phase " + (b ? "B" : "A") + " )";
                } else if((b && c2ph.EndsWith("A )")) || (!b && c2ph.EndsWith("B )"))) {
                    c2ph = "( Mixed Phase )";
                }
            } 
            #endregion
        }

        #region Apply checks if all are enabled
        c1enable.enabled = c1allEnabled;
        c2enable.enabled = c2allEnabled; 
        #endregion
        #region Show phase if any are enabled
        if(c1phase != null) c1phase.text = (c1anyEnabled ? c1ph : "");
        if(c2phase != null) c2phase.text = (c2anyEnabled ? c2ph : ""); 
        #endregion

        #region Color 1 pattern
        if(c1patt != null) {
            if(c1anyEnabled) {
                bool foundOne = false;
                Pattern p = null;

                foreach(LightHead alpha in cam.SelectedHead) {
                    if(!alpha.gameObject.activeInHierarchy) continue;

                    Pattern ap = alpha.GetPattern(func, false);
                    if(!foundOne) {
                        p = ap;
                        foundOne = true;
                    } else if(p != ap) {
                        p = null;
                        break;
                    }
                }

                if(!foundOne) {
                    c1patt.text = "";
                } else if(p == null) {
                    c1patt.text = "Multiple Patterns";
                } else {
                    c1patt.text = p.name;
                }
            } else {
                c1patt.text = "";
            }
        } 
        #endregion

        #region Color 2 pattern
        if(c2patt != null) {
            if(c2anyEnabled) {
                bool foundOne = false;
                Pattern p = null;

                foreach(LightHead alpha in cam.SelectedHead) {
                    if(!alpha.gameObject.activeInHierarchy) continue;

                    Pattern ap = alpha.GetPattern(func, true);
                    if(!foundOne) {
                        p = ap;
                        foundOne = true;
                    } else if(p != ap) {
                        p = null;
                        break;
                    }
                }

                if(!foundOne) {
                    c2patt.text = "";
                } else if(p == null) {
                    c2patt.text = "Multiple Patterns";
                } else {
                    c2patt.text = p.name;
                }
            } else {
                c2patt.text = "";
            }
        } 
        #endregion

        #region Dimmer Percentage display
        if(dim != null) {
            dim.text = "Dim to " + BarManager.inst.patts.Get<NbtCompound>("dim")["dimp"].ShortValue + "%";
        } 
        #endregion
    }
}
