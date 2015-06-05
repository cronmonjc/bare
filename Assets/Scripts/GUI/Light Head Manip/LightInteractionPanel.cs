using UnityEngine;
using System.Collections;

public class LightInteractionPanel : MonoBehaviour {
    private enum ShowState {
        SUMMARY, OPTICS, FUNCASSIGN, FUNCEDIT
    }
    private static CameraControl cam;
    public static bool EditingFunc = false;
    private ShowState _state = ShowState.SUMMARY;
    private ShowState state {
        set { _state = value; Set(); }
        get { return _state; }
    }
    public GameObject SummaryPane, OpticPane, FuncAssignPane, FuncEditPane;

    // Update is called once per frame
    void Update() {
        if(cam == null) {
            cam = FindObjectOfType<CameraControl>();
            Set();
        }

        switch(state) {
            case ShowState.SUMMARY:
                if(cam.OnlyCamSelected.Count > 0) {
                    state = ShowState.OPTICS;
                } else if(EditingFunc) {
                    state = ShowState.FUNCASSIGN;
                }
                break;
            case ShowState.OPTICS:
                if(cam.OnlyCamSelected.Count == 0) {
                    state = ShowState.SUMMARY;
                } else if(EditingFunc) {
                    state = ShowState.FUNCASSIGN;
                }
                break;
            case ShowState.FUNCASSIGN:
                if(!EditingFunc) {
                    state = ShowState.SUMMARY;
                }
                break;
            case ShowState.FUNCEDIT:
            default:
                break;
        }
    }

    private void Set() {
        SummaryPane.SetActive(state == ShowState.SUMMARY);
        OpticPane.SetActive(state == ShowState.OPTICS);
        FuncAssignPane.SetActive(state == ShowState.FUNCASSIGN);
        FuncEditPane.SetActive(state == ShowState.FUNCEDIT);
    }
}
