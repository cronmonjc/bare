using UnityEngine;
using System.Collections;

public class LightInteractionPanel : MonoBehaviour {
    public enum ShowState {
        SUMMARY, OPTICS, FUNCASSIGN, FUNCEDIT, LENSES
    }
    private static CameraControl cam;
    public static bool EditingFunc = false;
    private ShowState _state = ShowState.SUMMARY;
    public ShowState state {
        set { _state = value; Set(); }
        get { return _state; }
    }
    public GameObject SummaryPane, LensPane, OpticPane, FuncAssignPane, FuncEditPane;
    public UnityEngine.UI.Text PattEditButton;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(cam == null) {
            cam = FindObjectOfType<CameraControl>();
            Set();
        }

        switch(state) {
            case ShowState.SUMMARY:
                if(cam.OnlyCamSelectedHead.Count > 0) {
                    state = ShowState.OPTICS;
                } else if(cam.SelectedLens.Count > 0) {
                    state = ShowState.LENSES;
                } else if(EditingFunc) {
                    state = ShowState.FUNCASSIGN;
                }
                break;
            case ShowState.OPTICS:
                if(cam.OnlyCamSelectedHead.Count == 0) {
                    state = ShowState.SUMMARY;
                } else if(cam.SelectedLens.Count > 0) {
                    state = ShowState.LENSES;
                } else if(EditingFunc) {
                    state = ShowState.FUNCASSIGN;
                }
                break;
            case ShowState.LENSES:
                if(cam.SelectedLens.Count == 0) {
                    state = ShowState.SUMMARY;
                } else if(cam.OnlyCamSelectedHead.Count > 0) {
                    state = ShowState.OPTICS;
                } else if(EditingFunc) {
                    state = ShowState.FUNCASSIGN;
                }
                break;
            case ShowState.FUNCASSIGN:
                if(!EditingFunc) {
                    state = ShowState.SUMMARY;
                } else {
                    if(FunctionEditPane.currFunc != AdvFunction.NONE) {
                        state = ShowState.FUNCEDIT;
                    }
                }
                break;
            case ShowState.FUNCEDIT:
                if(!EditingFunc) {
                    state = ShowState.SUMMARY;
                } else if(FunctionEditPane.currFunc == AdvFunction.NONE) {
                    state = ShowState.FUNCASSIGN;
                }
                break;
            default:
                break;
        }
    }

    private void Set() {
        SummaryPane.SetActive(state == ShowState.SUMMARY);
        OpticPane.SetActive(state == ShowState.OPTICS);
        LensPane.SetActive(state == ShowState.LENSES);
        FuncAssignPane.SetActive(state == ShowState.FUNCASSIGN);
        FuncEditPane.SetActive(state == ShowState.FUNCEDIT);
    }

    public void ToggleEditFunc() {
        EditingFunc = !EditingFunc;
        FunctionEditPane.currFunc = AdvFunction.NONE;
        LightLabel.showPatt = false;

        foreach(LightLabel ll in FindObjectsOfType<LightLabel>()) {
            ll.Refresh();
        }
        if(PattEditButton != null) {
            PattEditButton.text = (EditingFunc ? "Go to Light Editing" : "Go to Pattern Editing");
        }
    }
}
