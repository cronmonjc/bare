using UnityEngine;
using System.Collections;

public class DeselectHint : MonoBehaviour {
    public static CameraControl cam;
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
