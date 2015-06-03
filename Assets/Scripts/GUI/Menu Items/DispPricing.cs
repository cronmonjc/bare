using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DispPricing : Selectable, IPointerClickHandler {
    public Image ToggleImage;

    void Update() {
        if(ToggleImage != null)
            ToggleImage.enabled = CameraControl.ShowPricing;
    }

    public void OnPointerClick(PointerEventData eventData) {
        CameraControl.ShowPricing = !CameraControl.ShowPricing;
    }
}
