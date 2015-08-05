using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;

/// <summary>
/// GUI Item.  Used to display what pattern selected head(s) are using.
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

    public void Refresh() {
        if(cam == null) { cam = FindObjectOfType<CameraControl>(); }

        NbtCompound cmpd = BarManager.inst.patts.Get<NbtCompound>(BarManager.GetFnString(cam.OnlyCamSelectedHead[0].transform, func));
        bool c1anyEnabled = false, c1allEnabled = true;
        bool c2anyEnabled = false, c2allEnabled = true;
        string c1ph = "", c2ph = "";

        //c2enable.enabled = c1enable.enabled = true;
        //if(c1phase != null) c1phase.text = "";
        //if(c2phase != null) c2phase.text = "";

        foreach(LightHead alpha in cam.OnlyCamSelectedHead) {
            if(!alpha.gameObject.activeInHierarchy) continue;

            byte bit = alpha.Bit;

            bool thisEnabled = false;

            if(cmpd.Contains("e" + (alpha.transform.position.y < 0 ? "r" : "f") + "1")) {
                thisEnabled = (cmpd["e" + (alpha.transform.position.y < 0 ? "r" : "f") + "1"].ShortValue & (0x1 << bit)) > 0;
                c1allEnabled &= thisEnabled;
                c1anyEnabled |= thisEnabled;
            }
            if(cmpd.Contains("e" + (alpha.transform.position.y < 0 ? "r" : "f") + "2")) {
                thisEnabled = (cmpd["e" + (alpha.transform.position.y < 0 ? "r" : "f") + "2"].ShortValue & (0x1 << bit)) > 0;
                c2allEnabled &= thisEnabled;
                c2anyEnabled |= thisEnabled;
            }

            if(c1phase != null) {
                bool b = (cmpd.Get<NbtShort>("p" + (alpha.transform.position.y < 0 ? "r" : "f") + "1").ShortValue & (0x1 << bit)) > 0;

                if(c1ph == "") {
                    c1ph = "( Phase " + (b ? "B" : "A") + " )";
                } else if((b && c1ph.EndsWith("A )")) || (!b && c1ph.EndsWith("B )"))) {
                    c1ph = "( Mixed Phase )";
                }
            }

            if(c2phase != null) {
                bool b = (cmpd.Get<NbtShort>("p" + (alpha.transform.position.y < 0 ? "r" : "f") + "2").ShortValue & (0x1 << bit)) > 0;

                if(c2ph == "") {
                    c2ph = "( Phase " + (b ? "B" : "A") + " )";
                } else if((b && c2ph.EndsWith("A )")) || (!b && c2ph.EndsWith("B )"))) {
                    c2ph = "( Mixed Phase )";
                }
            }
        }

        c1enable.enabled = c1allEnabled;
        c2enable.enabled = c2allEnabled;
        if(c1phase != null) c1phase.text = (c1anyEnabled ? c1ph : "");
        if(c2phase != null) c2phase.text = (c2anyEnabled ? c2ph : "");

        if(c1patt != null) {
            if(c1anyEnabled) {
                bool foundOne = false;
                Pattern p = null;

                foreach(LightHead alpha in cam.OnlyCamSelectedHead) {
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

        if(c2patt != null) {
            if(c2anyEnabled) {
                bool foundOne = false;
                Pattern p = null;

                foreach(LightHead alpha in cam.OnlyCamSelectedHead) {
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

        if(dim != null) {
            dim.text = "Dim to " + cmpd["dimp"].ShortValue + "%";
        }
    }
}
