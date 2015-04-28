using UnityEngine;
using System.Collections;

public class LightInteractionPanel : MonoBehaviour {
    private static CameraControl cam;
    private bool show;

    void Start() {
        show = false;
        foreach(Transform alpha in transform) {
            alpha.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update() {
        if(cam == null) {
            cam = FindObjectOfType<CameraControl>();
        }

        if(show && cam.OnlyCamSelected.Count == 0) {
            show = false;
            foreach(Transform alpha in transform) {
                alpha.gameObject.SetActive(false);
            }
        } else if(!show && cam.OnlyCamSelected.Count > 0) {
            show = true;
            foreach(Transform alpha in transform) {
                alpha.gameObject.SetActive(true);
            }
        }
    }
}
