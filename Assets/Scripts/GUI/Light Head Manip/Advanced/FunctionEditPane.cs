using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FunctionEditPane : MonoBehaviour {
    private CameraControl cam;

    public static AdvFunction currFunc;
    public enum ShowState {
        NONE, FLASHING, DIMMER, TRAFFIC
    }
    public Text funcName, funcType;
    private Text previewText;
    public GameObject paneParent, flashing, dimmer, traffic, otherHeadsWarn, testFlashing;
    private ShowState _state, funcState;
    public ShowState state {
        get {
            return _state;
        }
        set {
            _state = value;
            Set();
        }
    }

    void OnEnable() {
        RetestStatic();
        if(previewText == null) {
            previewText = testFlashing.GetComponentInChildren<Text>();
        }

        paneParent.SetActive(false);
        switch(currFunc) {
            case AdvFunction.PRIO1:
            case AdvFunction.PRIO2:
            case AdvFunction.PRIO3:
            case AdvFunction.PRIO4:
            case AdvFunction.PRIO5:
            case AdvFunction.FTAKEDOWN:
            case AdvFunction.FALLEY:
            case AdvFunction.ICL:
                state = ShowState.FLASHING;
                break;
            case AdvFunction.TRAFFIC_LEFT:
            case AdvFunction.TRAFFIC_RIGHT:
                state = ShowState.TRAFFIC;
                break;
            case AdvFunction.DIM:
                state = ShowState.DIMMER;
                break;
            default:
                state = ShowState.NONE;
                break;
        }

        switch(currFunc) {
            case AdvFunction.PRIO1:
            case AdvFunction.PRIO2:
            case AdvFunction.PRIO3:
            case AdvFunction.PRIO4:
            case AdvFunction.PRIO5:
            case AdvFunction.FTAKEDOWN:
            case AdvFunction.FALLEY:
            case AdvFunction.ICL:
                funcType.text = "Flashing";
                testFlashing.SetActive(true);
                break;
            case AdvFunction.TAKEDOWN:
            case AdvFunction.ALLEY_LEFT:
            case AdvFunction.ALLEY_RIGHT:
            case AdvFunction.TURN_LEFT:
            case AdvFunction.TURN_RIGHT:
            case AdvFunction.TAIL:
            case AdvFunction.T13:
            case AdvFunction.EMITTER:
                funcType.text = "Steady Burn";
                testFlashing.SetActive(false);
                break;
            case AdvFunction.CRUISE:
                funcType.text = "Cruise";
                testFlashing.SetActive(false);
                break;
            case AdvFunction.TRAFFIC_LEFT:
            case AdvFunction.TRAFFIC_RIGHT:
                funcType.text = "Traffic Director";
                testFlashing.SetActive(true);
                break;
            case AdvFunction.DIM:
                funcType.text = "Dimmer";
                testFlashing.SetActive(false);
                break;
            default:
                funcType.text = "???";
                testFlashing.SetActive(false);
                break;
        }

        switch(currFunc) {
            case AdvFunction.TAKEDOWN:
                funcName.text = "Takedown";
                break;
            case AdvFunction.ALLEY_LEFT:
                funcName.text = "Alley Left";
                break;
            case AdvFunction.ALLEY_RIGHT:
                funcName.text = "Alley Right";
                break;
            case AdvFunction.TURN_LEFT:
                funcName.text = "Turn Left";
                break;
            case AdvFunction.TURN_RIGHT:
                funcName.text = "Turn Right";
                break;
            case AdvFunction.TAIL:
                funcName.text = "Brake Lights";
                break;
            case AdvFunction.T13:
                funcName.text = "California T13 Steady";
                break;
            case AdvFunction.EMITTER:
                funcName.text = "Emitter";
                break;
            case AdvFunction.PRIO1:
                funcName.text = "Priority 1";
                break;
            case AdvFunction.PRIO2:
                funcName.text = "Priority 2";
                break;
            case AdvFunction.PRIO3:
                funcName.text = "Priority 3";
                break;
            case AdvFunction.PRIO4:
                funcName.text = "Priority 4";
                break;
            case AdvFunction.PRIO5:
                funcName.text = "Priority 5";
                break;
            case AdvFunction.FTAKEDOWN:
                funcName.text = "Flashing Pursuit";
                break;
            case AdvFunction.FALLEY:
                funcName.text = "Flashing Alley";
                break;
            case AdvFunction.ICL:
                funcName.text = "ICL";
                break;
            case AdvFunction.TRAFFIC_LEFT:
                funcName.text = "Direct Left";
                break;
            case AdvFunction.TRAFFIC_RIGHT:
                funcName.text = "Direct Right";
                break;
            case AdvFunction.CRUISE:
                funcName.text = "Cruise";
                break;
            case AdvFunction.DIM:
                funcName.text = "Dimmer";
                break;
            default:
                funcName.text = "???";
                break;
        }

        if(testFlashing.activeInHierarchy) {
            previewText.text = "Preview " + funcName.text;
            if(currFunc == AdvFunction.FTAKEDOWN || currFunc == AdvFunction.FALLEY) {
                for(byte i = 0; i < 20; i++) {
                    if(FnDragTarget.inputMap.Value[i] == 0xC00) {
                        previewText.text = "Preview Flashing\nAlley & Pursuit";
                    }
                }
            }
        }
    }

    void Update() {
        if(cam == null) cam = FindObjectOfType<CameraControl>();

        paneParent.SetActive(cam.OnlyCamSelectedHead.Count > 0);
    }

    private void Set() {
        flashing.SetActive(state == ShowState.FLASHING);
        //dimmer.SetActive(state == ShowState.DIMMER);
        dimmer.SetActive(false);
        traffic.SetActive(state == ShowState.TRAFFIC);
    }

    public void Retest() {
        otherHeadsWarn.SetActive(false);

        foreach(FuncPattSelect fps in transform.GetComponentsInChildren<FuncPattSelect>(true)) {
            fps.Refresh();
        }

        List<byte> front = new List<byte>(), back = new List<byte>();
        List<string> patts = new List<string>();
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;

            byte bit = alpha.Bit;
            if(alpha.transform.position.y < 0) {
                if(!back.Contains(bit)) {
                    back.Add(bit);
                }
            } else {
                if(!front.Contains(bit)) {
                    front.Add(bit);
                }
            }

            string tagname = alpha.transform.position.y < 0 ? "r" : "f";
            string path = alpha.transform.GetPath();

            if(path.Contains("C") || path.Contains("A")) {
                tagname = tagname + "cor";
            } else if(path.Contains("I")) {
                tagname = tagname + "inb";
            } else if(path.Contains("O")) {
                if(alpha.loc == Location.FAR_REAR)
                    tagname = tagname + "far";
                else
                    tagname = tagname + "oub";
            } else if(path.Contains("N") || path.Split('/')[2].EndsWith("F")) {
                tagname = tagname + "cen";
            }

            if(!patts.Contains(tagname)) {
                patts.Add(tagname);
            }
        }
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.hasRealHead || alpha.Selected) continue;

            byte bit = alpha.Bit;
            if(alpha.transform.position.y < 0) {
                if(back.Contains(bit)) {
                    otherHeadsWarn.SetActive(true);
                    return;
                }
            } else {
                if(front.Contains(bit)) {
                    otherHeadsWarn.SetActive(true);
                    return;
                }
            }

            string tagname = alpha.transform.position.y < 0 ? "r" : "f";
            string path = alpha.transform.GetPath();

            if(path.Contains("C") || path.Contains("A")) {
                tagname = tagname + "cor";
            } else if(path.Contains("I")) {
                tagname = tagname + "inb";
            } else if(path.Contains("O")) {
                if(alpha.loc == Location.FAR_REAR)
                    tagname = tagname + "far";
                else
                    tagname = tagname + "oub";
            } else if(path.Contains("N") || path.Split('/')[2].EndsWith("F")) {
                tagname = tagname + "cen";
            }

            if(patts.Contains(tagname)) {
                otherHeadsWarn.SetActive(true);
                return;
            }
        }
    }

    public static void RetestStatic() {
        if(FunctionEditPane.currFunc == AdvFunction.NONE) return;

        if(FuncEnable.clr1 != null) FuncEnable.clr1.Retest();
        if(FuncEnable.clr2 != null) FuncEnable.clr2.Retest();
        if(FuncPhase.ph1 != null) FuncPhase.ph1.Retest();
        if(FuncPhase.ph2 != null) FuncPhase.ph2.Retest();

        FindObjectOfType<FunctionEditPane>().Retest();
    }
}
