using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;

public class AdvFuncDisp : MonoBehaviour {
    public static CameraControl cam;
    public AdvFunction func;

    private Text c1patt, c2patt, c1phase, c2phase, dim;
    private Image c1enable, c2enable;

    void Start() {
        c1enable = transform.FindChild("C1Enable").FindChild("Image").GetComponent<Image>();
        c2enable = transform.FindChild("C2Enable").FindChild("Image").GetComponent<Image>();

        switch(func) {
            case AdvFunction.TRAFFIC_LEFT:
            case AdvFunction.TRAFFIC_RIGHT:
                c1patt = transform.FindChild("C1Patt").GetComponent<Text>();
                c2patt = transform.FindChild("C2Patt").GetComponent<Text>();
                break;
            case AdvFunction.LEVEL1:
            case AdvFunction.LEVEL2:
            case AdvFunction.LEVEL3:
            case AdvFunction.LEVEL4:
            case AdvFunction.LEVEL5:
            case AdvFunction.FTAKEDOWN:
            case AdvFunction.FALLEY:
            case AdvFunction.ICL:
                c1patt = transform.FindChild("C1Patt").GetComponent<Text>();
                c2patt = transform.FindChild("C2Patt").GetComponent<Text>();
                c1phase = transform.FindChild("C1Phase").GetComponent<Text>();
                c2phase = transform.FindChild("C2Phase").GetComponent<Text>();
                break;
            case AdvFunction.DIM:
                dim = transform.FindChild("Dimp").GetComponent<Text>();
                break;
            default:
                break;
        }

    }

    public void Refresh() {
        if(cam == null) { cam = FindObjectOfType<CameraControl>(); Start(); }

        NbtCompound cmpd = BarManager.inst.patts.Get<NbtCompound>(BarManager.GetFnString(cam.OnlyCamSelected[0].transform, func));
        c2enable.enabled = c1enable.enabled = true;
        if(c1phase != null) c1phase.text = "";
        if(c2phase != null) c2phase.text = "";

        foreach(LightHead alpha in cam.OnlyCamSelected) {
            if(!alpha.gameObject.activeInHierarchy) continue;

            byte bit = alpha.Bit;

            if(cmpd.Contains("e" + (alpha.transform.position.y < 0 ? "r" : "f") + "1")) {
                c1enable.enabled &= (cmpd["e" + (alpha.transform.position.y < 0 ? "r" : "f") + "1"].ShortValue & (0x1 << bit)) > 0;
            }
            if(cmpd.Contains("e" + (alpha.transform.position.y < 0 ? "r" : "f") + "2")) {
                c2enable.enabled &= (cmpd["e" + (alpha.transform.position.y < 0 ? "r" : "f") + "2"].ShortValue & (0x1 << bit)) > 0;
            }

            if(c1phase != null) {
                bool b = (cmpd.Get<NbtShort>("p" + (alpha.transform.position.y < 0 ? "r" : "f") + "1").ShortValue & (0x1 << bit)) > 0;

                if(c1phase.text == "") {
                    c1phase.text = "( Phase " + (b ? "B" : "A") + " )";
                } else if((b && c1phase.text.EndsWith("A )")) || (!b && c1phase.text.EndsWith("B )"))) {
                    c1phase.text = "( Mixed Phase )";
                }
            }

            if(c2phase != null) {
                bool b = (cmpd.Get<NbtShort>("p" + (alpha.transform.position.y < 0 ? "r" : "f") + "2").ShortValue & (0x1 << bit)) > 0;

                if(c2phase.text == "") {
                    c2phase.text = "( Phase " + (b ? "B" : "A") + " )";
                } else if((b && c2phase.text.EndsWith("A )")) || (!b && c2phase.text.EndsWith("B )"))) {
                    c2phase.text = "( Mixed Phase )";
                }
            }
        }

        if(c1patt != null) {
            bool foundOne = false;
            Pattern p = null;

            foreach(LightHead alpha in cam.OnlyCamSelected) {
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
        }

        if(c2patt != null) {
            bool foundOne = false;
            Pattern p = null;

            foreach(LightHead alpha in cam.OnlyCamSelected) {
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
        }

        if(dim != null) {
            dim.text = "Dim to " + cmpd["dimp"].ShortValue + "%";
        }
    }
}
