using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Controls the display of the various panes of light modification.
/// </summary>
public class LightInteractionPanel : MonoBehaviour {
    /// <summary>
    /// An enumeration of all of the different panes that could be shown
    /// </summary>
    public enum ShowState {
        SUMMARY, OPTICS, FUNCASSIGN, FUNCEDIT, LENSES
    }
    /// <summary>
    /// A reference to the CameraControl component
    /// </summary>
    private static CameraControl cam;
    /// <summary>
    /// Are we currently editing functions?
    /// </summary>
    public static bool EditingFunc = false;
    /// <summary>
    /// The current state of the panel.
    /// </summary>
    private ShowState _state = ShowState.SUMMARY;
    /// <summary>
    /// Gets or sets the current display state of the panel.
    /// </summary>
    public ShowState state {
        set { _state = value; Set(); }
        get { return _state; }
    }
    /// <summary>
    /// The reference to the summary pane GameObject.  Set via Unity Inspector.
    /// </summary>
    public GameObject SummaryPane;
    /// <summary>
    /// The reference to the lens pane GameObject.  Set via Unity Inspector.
    /// </summary>
    public GameObject LensPane;
    /// <summary>
    /// The reference to the optic pane GameObject.  Set via Unity Inspector.
    /// </summary>
    public GameObject OpticPane;
    /// <summary>
    /// The reference to the function assign pane GameObject.  Set via Unity Inspector.
    /// </summary>
    public GameObject FuncAssignPane;
    /// <summary>
    /// The reference to the function edit pane GameObject.  Set via Unity Inspector.
    /// </summary>
    public GameObject FuncEditPane;
    /// <summary>
    /// The reference to the pattern edit button Text UI Component.  Set via Unity Inspector.
    /// </summary>
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
                if(cam.SelectedHead.Count > 0) {
                    state = ShowState.OPTICS;
                } else if(cam.SelectedLens.Count > 0) {
                    state = ShowState.LENSES;
                } else if(EditingFunc) {
                    state = ShowState.FUNCASSIGN;
                }
                break;
            case ShowState.OPTICS:
                if(cam.SelectedHead.Count == 0) {
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
                } else if(cam.SelectedHead.Count > 0) {
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

    /// <summary>
    /// Sets this Component's pane display.
    /// </summary>
    private void Set() {
        SummaryPane.SetActive(state == ShowState.SUMMARY);
        OpticPane.SetActive(state == ShowState.OPTICS);
        LensPane.SetActive(state == ShowState.LENSES);
        FuncAssignPane.SetActive(state == ShowState.FUNCASSIGN);
        FuncEditPane.SetActive(state == ShowState.FUNCEDIT);
    }

    /// <summary>
    /// Toggles whether or not we're editing functions.  Called by the "Go to Function Editing" button.
    /// </summary>
    public void ToggleEditFunc() {
        EditingFunc = !EditingFunc;
        FunctionEditPane.currFunc = AdvFunction.NONE;
        LightLabel.showPatt = false;

        cam.SelectedHead.Clear();
        cam.SelectedLens.Clear();

        foreach(LightLabel ll in FindObjectsOfType<LightLabel>()) {
            ll.Refresh();
        }
        if(PattEditButton != null) {
            PattEditButton.text = (EditingFunc ? "Go to Light Editing" : "Go to Pattern Editing");
        }
    }
}
