using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI Component.  Allows for and manages the modification of a certain function's patterns, enables, and phases.
/// </summary>
public class FunctionEditPane : MonoBehaviour {
    /// <summary>
    /// The CameraControl reference, to make finding selected head count easier
    /// </summary>
    private CameraControl cam;

    /// <summary>
    /// The function currently being edited
    /// </summary>
    public static AdvFunction currFunc;
    /// <summary>
    /// The previous function, used to refresh everything when changing functions
    /// </summary>
    private AdvFunction prevFunc = AdvFunction.NONE;
    /// <summary>
    /// Enumeration of all possible configurations of the Function Edit Pane.
    /// </summary>
    public enum ShowState {
        NONE, FLASHING, DIMMER, TRAFFIC
    }
    /// <summary>
    /// The reference to the function name Text Component.  Set via Unity Inspector.
    /// </summary>
    public Text funcName;
    /// <summary>
    /// The reference to the function type Text Component.  Set via Unity Inspector.
    /// </summary>
    public Text funcType;
    /// <summary>
    /// The reference to the preview text Text Component.
    /// </summary>
    private Text previewText;
    /// <summary>
    /// The reference to the pane parent GameObject.  Set via Unity Inspector.
    /// </summary>
    public GameObject paneParent;
    /// <summary>
    /// The reference to the flashing pane GameObject.  Set via Unity Inspector.
    /// </summary>
    public GameObject flashing;
    /// <summary>
    /// The reference to the dimmer pane GameObject.  Set via Unity Inspector.  Deprecated
    /// </summary>
    public GameObject dimmer;
    /// <summary>
    /// The reference to the traffic director pane GameObject.  Set via Unity Inspector.
    /// </summary>
    public GameObject traffic;
    /// <summary>
    /// The reference to the traffic option pane GameObject.  Set via Unity Inspector.
    /// </summary>
    public GameObject trafficOpt;
    /// <summary>
    /// The reference to the other heads warn GameObject.  Set via Unity Inspector.
    /// </summary>
    public GameObject otherHeadsWarn;
    /// <summary>
    /// The reference to the test flashing GameObject.  Set via Unity Inspector.
    /// </summary>
    public GameObject testFlashing;
    /// <summary>
    /// The current state of display
    /// </summary>
    private ShowState _state;
    public ShowState state {
        get {
            return _state;
        }
        set {
            _state = value;
            Set();
        }
    }

    /// <summary>
    /// Called immediately when the Component's GameObject is enabled
    /// </summary>
    void OnEnable() {
        prevFunc = currFunc;

        RetestStatic();
        if(previewText == null) {
            previewText = testFlashing.transform.Find("Label").GetComponent<Text>();
        }

        paneParent.SetActive(false);
        #region Figure out which pane to show
        if(LightDict.flashingFuncs.Contains(currFunc)) {
            state = ShowState.FLASHING;
        } else switch(currFunc) {
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
        #endregion

        #region Figure out what the type of function is
        if(LightDict.flashingFuncs.Contains(currFunc)) {
            funcType.text = "Flashing";
            testFlashing.SetActive(true);
        } else switch(currFunc) {
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
        #endregion

        #region Display the name of the function
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
                funcName.text = "Tail Lights";
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
        #endregion

        #region Have Preview Text show name of function as well.
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
        #endregion
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(cam == null) cam = FindObjectOfType<CameraControl>();

        paneParent.SetActive(cam.SelectedHead.Count > 0);

        if(prevFunc != currFunc) {
            OnEnable();
        }
    }

    /// <summary>
    /// Sets the pane to show.
    /// </summary>
    private void Set() {
        flashing.SetActive(state == ShowState.FLASHING);
        //dimmer.SetActive(state == ShowState.DIMMER);
        dimmer.SetActive(false);
        traffic.SetActive(state == ShowState.TRAFFIC);
        trafficOpt.SetActive(state == ShowState.TRAFFIC);
    }

    /// <summary>
    /// Retests whether or not to show the Other Heads Warn GameObject, as well as refreshing the patterns.
    /// </summary>
    public void Retest() {
        otherHeadsWarn.SetActive(false); // Disable by default

        #region Refresh FuncPattSelect Components
        switch(state) {
            case ShowState.FLASHING:
                foreach(FuncPattSelect fps in flashing.GetComponentsInChildren<FuncPattSelect>(true)) {
                    fps.Refresh();
                }
                break;
            case ShowState.TRAFFIC:
                foreach(FuncPattSelect fps in traffic.GetComponentsInChildren<FuncPattSelect>(true)) {
                    fps.Refresh();
                }
                break;
            default:
                break;
        } 
        #endregion

        #region Setup
        List<byte> front = new List<byte>(), back = new List<byte>();
        List<string> patts = new List<string>(); 
        #endregion
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue; // Only for visible and selected heads

            #region Collect selected head bits
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
            #endregion

            #region Collect selected pattern references
            string tagname = alpha.transform.position.y < 0 ? "r" : "f";
            string path = alpha.Path;

            if(path.Contains("C") || path.Contains("A")) {
                tagname = tagname + "cor";
            } else if(path.Contains("I")) {
                tagname = tagname + "inb";
            } else if(path.Contains("O")) {
                if(alpha.isFar)
                    tagname = tagname + "far";
                else
                    tagname = tagname + "oub";
            } else if(path.Contains("N") || path.Split('/')[2].EndsWith("F")) {
                tagname = tagname + "cen";
            }

            if(!patts.Contains(tagname)) {
                patts.Add(tagname);
            } 
            #endregion
        }
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.hasRealHead || alpha.Selected) continue; // Only for visible but unselected heads

            #region If this unselected head shares a bit with a selected head, show warning
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
            #endregion

            #region If this unselected head shares a pattern reference with a selected head, show warning
            string tagname = alpha.transform.position.y < 0 ? "r" : "f";
            string path = alpha.Path;

            if(path.Contains("C") || path.Contains("A")) {
                tagname = tagname + "cor";
            } else if(path.Contains("I")) {
                tagname = tagname + "inb";
            } else if(path.Contains("O")) {
                if(alpha.isFar)
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
            #endregion
        }
    }

    /// <summary>
    /// Retests the Component via a static method.
    /// </summary>
    public static void RetestStatic() {
        if(FunctionEditPane.currFunc == AdvFunction.NONE) return;

        if(FuncEnable.clr1 != null) FuncEnable.clr1.Retest();
        if(FuncEnable.clr2 != null) FuncEnable.clr2.Retest();
        if(FuncPhase.ph1 != null) FuncPhase.ph1.Retest();
        if(FuncPhase.ph2 != null) FuncPhase.ph2.Retest();

        FindObjectOfType<FunctionEditPane>().Retest();
    }
}
