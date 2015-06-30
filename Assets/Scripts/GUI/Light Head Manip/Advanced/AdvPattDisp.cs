using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AdvPattDisp : MonoBehaviour {
    private CameraControl cam;
    public Text color1, color2;
    public Dictionary<AdvFunction, AdvFuncDisp> displays;

    void Start() {
        if(displays == null) {
            displays = new Dictionary<AdvFunction, AdvFuncDisp>();
            foreach(AdvFuncDisp alpha in GetComponentsInChildren<AdvFuncDisp>(true)) {
                displays[alpha.func] = alpha;
            }
        }
    }

    public void Refresh() {
        if(displays == null) {
            displays = new Dictionary<AdvFunction, AdvFuncDisp>();
            foreach(AdvFuncDisp alpha in GetComponentsInChildren<AdvFuncDisp>(true)) {
                displays[alpha.func] = alpha;
            }
        }
        foreach(AdvFuncDisp alpha in displays.Values) {
            alpha.gameObject.SetActive(false);
        }

        HashSet<AdvFunction> funcs = new HashSet<AdvFunction>();

        if(cam == null) cam = FindObjectOfType<CameraControl>();

        color1.text = color2.text = "";

        foreach(LightHead alpha in cam.OnlyCamSelected) {
            if(alpha.lhd.style == null) continue;
            foreach(BasicFunction beta in alpha.lhd.funcs) {
                switch(beta) {
                    case BasicFunction.CAL_STEADY:
                        funcs.Add(AdvFunction.T13);
                        break;
                    case BasicFunction.CRUISE:
                        funcs.Add(AdvFunction.CRUISE);
                        break;
                    case BasicFunction.EMITTER:
                        funcs.Add(AdvFunction.EMITTER);
                        break;
                    case BasicFunction.FLASHING:
                        foreach(AdvFunction charlie in new AdvFunction[] { AdvFunction.LEVEL1, AdvFunction.LEVEL2, AdvFunction.LEVEL3, AdvFunction.LEVEL4, AdvFunction.LEVEL5, AdvFunction.FTAKEDOWN, AdvFunction.FALLEY, AdvFunction.ICL }) {
                            funcs.Add(charlie);
                        }
                        break;
                    case BasicFunction.STEADY:
                        funcs.Add(AdvFunction.TAKEDOWN);
                        funcs.Add(AdvFunction.ALLEY_LEFT);
                        funcs.Add(AdvFunction.ALLEY_RIGHT);
                        break;
                    case BasicFunction.STT:
                        funcs.Add(AdvFunction.TURN_LEFT);
                        funcs.Add(AdvFunction.TURN_RIGHT);
                        break;
                    case BasicFunction.TRAFFIC:
                        funcs.Add(AdvFunction.TRAFFIC_LEFT);
                        funcs.Add(AdvFunction.TRAFFIC_RIGHT);
                        break;
                    default:
                        break;
                }
            }
            funcs.Add(AdvFunction.DIM);

            string[] colors = alpha.lhd.style.name.Split('/', '\\');
            if(colors.Length == 2) {
                if(color2.text.Equals("")) {
                    color2.text = colors[1];
                } else if(!color2.text.Equals(colors[1], System.StringComparison.CurrentCultureIgnoreCase)) {
                    color2.text = "Color 2";
                }
            }
            if(color1.text.Equals("")) {
                color1.text = colors[0];
            } else if(!color1.text.Equals(colors[0], System.StringComparison.CurrentCultureIgnoreCase)) {
                color1.text = "Color 1";
            }
        }

        foreach(AdvFunction alpha in funcs) {
            displays[alpha].gameObject.SetActive(true);
            displays[alpha].Refresh();
        }
    }
}
