using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class FunctionSelect : MonoBehaviour, IPointerClickHandler {
    public static CameraControl cam;
    public Function myFunction;
    public Graphic ToggleImg;

    public bool isOn;

    public void OnPointerClick(PointerEventData eventData) {
        if(cam == null) cam = FindObjectOfType<CameraControl>();
        if(!isOn) {
            foreach(LightHead alpha in cam.OnlyCamSelected) {
                if(alpha.CapableFunctions.Contains(myFunction) && !alpha.selectedFunctions.Contains(myFunction)) {
                    alpha.selectedFunctions.Add(myFunction);
                }
            }
        } else {
            foreach(LightHead alpha in cam.OnlyCamSelected) {
                if(alpha.selectedFunctions.Contains(myFunction)) {
                    alpha.selectedFunctions.Remove(myFunction);
                }
            }
        }

        isOn = !isOn;
        ToggleImg.gameObject.SetActive(isOn);

        cam.os.Refresh();
    }

    public void Refresh() {
        if(cam == null) cam = FindObjectOfType<CameraControl>();
        isOn = false;
        bool first = true;

        Button b = GetComponent<Button>();
        b.interactable = false;

        foreach(LightHead alpha in cam.OnlyCamSelected) {
            if(alpha.CapableFunctions.Contains(myFunction)) {
                b.interactable = true;

                if(first) {
                    isOn = alpha.selectedFunctions.Contains(myFunction);
                    first = false;
                } else {
                    isOn &= alpha.selectedFunctions.Contains(myFunction);
                }
            }
        }

        ToggleImg.gameObject.SetActive(isOn);
    }
}
