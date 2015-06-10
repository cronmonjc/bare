using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FunctionEditPane : MonoBehaviour {
    private CameraControl cam;

    public AdvFunction currFunc;
    public enum ShowState {
        NONE, FLASHING, DIMMER, TRAFFIC
    }
    public Text funcName, funcType;
    public GameObject paneParent, flashing, dimmer, traffic;
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
        paneParent.SetActive(false);
        switch(currFunc) {
            case AdvFunction.LEVEL1:
            case AdvFunction.LEVEL2:
            case AdvFunction.LEVEL3:
            case AdvFunction.LEVEL4:
            case AdvFunction.LEVEL5:
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
            case AdvFunction.LEVEL1:
            case AdvFunction.LEVEL2:
            case AdvFunction.LEVEL3:
            case AdvFunction.LEVEL4:
            case AdvFunction.LEVEL5:
            case AdvFunction.FTAKEDOWN:
            case AdvFunction.FALLEY:
            case AdvFunction.ICL:
                funcType.text = "Flashing";
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
                break;
            case AdvFunction.CRUISE:
                funcType.text = "Cruise";
                break;
            case AdvFunction.TRAFFIC_LEFT:
            case AdvFunction.TRAFFIC_RIGHT:
                funcType.text = "Traffic Director";
                break;
            case AdvFunction.DIM:
                funcType.text = "Dimmer";
                break;
            default:
                funcType.text = "???";
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
            case AdvFunction.LEVEL1:
                funcName.text = "Level 1";
                break;
            case AdvFunction.LEVEL2:
                funcName.text = "Level 2";
                break;
            case AdvFunction.LEVEL3:
                funcName.text = "Level 3";
                break;
            case AdvFunction.LEVEL4:
                funcName.text = "Level 4";
                break;
            case AdvFunction.LEVEL5:
                funcName.text = "Level 5";
                break;
            case AdvFunction.FTAKEDOWN:
                funcName.text = "Flashing Takedown";
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
    }

    void Update() {
        if(cam == null) cam = FindObjectOfType<CameraControl>();

        paneParent.SetActive(cam.OnlyCamSelected.Count > 0);
    }

    private void Set() {
        flashing.SetActive(state == ShowState.FLASHING);
        dimmer.SetActive(state == ShowState.DIMMER);
        traffic.SetActive(state == ShowState.TRAFFIC);
    }
}
