using UnityEngine;
using System.Collections;

public class LightInteractionPanel : MonoBehaviour {
    private enum ShowState {
        SUMMARY, OPTICS
    }
    private static CameraControl cam;
    private ShowState state = ShowState.SUMMARY;
    public GameObject SummaryPane, OpticPane;

    // Update is called once per frame
    void Update() {
        if(cam == null) {
            cam = FindObjectOfType<CameraControl>();
            SummaryPane.SetActive(true);
            OpticPane.SetActive(false);
        }

        if(cam.OnlyCamSelected.Count == 0) {
            if(state != ShowState.SUMMARY) {
                state = ShowState.SUMMARY;
                SummaryPane.SetActive(true);
                OpticPane.SetActive(false);
            }
        } else {
            if(state != ShowState.OPTICS) {
                state = ShowState.OPTICS;
                SummaryPane.SetActive(false);
                OpticPane.SetActive(true);
            }
        }
    }
}
