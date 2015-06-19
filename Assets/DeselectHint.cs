using UnityEngine;
using System.Collections;

public class DeselectHint : MonoBehaviour {
    public static CameraControl cam;
    public GameObject[] showForDeselect;

    void Update() {
        if(cam == null) cam = FindObjectOfType<CameraControl>();
        bool show = cam.OnlyCamSelected.Count > 0 && cam.lip.state != LightInteractionPanel.ShowState.FUNCEDIT;
        foreach(GameObject go in showForDeselect) {
            go.SetActive(show);
        }
    }
}
