using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DispPricing : Selectable, IPointerClickHandler {
    public Image ToggleImage;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(ToggleImage != null)
            ToggleImage.enabled = CameraControl.ShowPricing;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if(interactable) {
            CameraControl.ShowPricing = !CameraControl.ShowPricing;
        }
    }
}
