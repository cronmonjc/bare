using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// UI Component.  Display Pricing toggle.
/// </summary>
public class DispPricing : Selectable, IPointerClickHandler {
    /// <summary>
    /// The toggle image.  Set via Unity Inspector.
    /// </summary>
    public Image ToggleImage;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(ToggleImage != null)
            ToggleImage.enabled = CameraControl.ShowPricing;
    }

    /// <summary>
    /// Called when the user clicks on an object.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnPointerClick(PointerEventData eventData) {
        if(interactable) {
            CameraControl.ShowPricing = !CameraControl.ShowPricing;
        }
    }
}
