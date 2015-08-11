using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Displays the deselection hint.
/// </summary>
public class DeselectHint : MonoBehaviour {
    /// <summary>
    /// Reference to the CameraControl, for use in finding selection count
    /// </summary>
    public static CameraControl cam;
    /// <summary>
    /// The list of GameObjects to show when objects are selected, or to hide when no objects are selected
    /// </summary>
    public GameObject[] showForDeselect;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(cam == null) cam = FindObjectOfType<CameraControl>();
        bool show = (cam.SelectedHead.Count > 0 || cam.SelectedLens.Count > 0) && cam.lip.state != LightInteractionPanel.ShowState.FUNCEDIT;
        foreach(GameObject go in showForDeselect) {
            go.SetActive(show);
        }
    }
}
