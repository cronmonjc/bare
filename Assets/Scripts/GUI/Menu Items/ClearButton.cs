using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// UI Component.  Clears the bar when clicked.
/// </summary>
public class ClearButton : MonoBehaviour, IPointerClickHandler {
    /// <summary>
    /// Called when the user clicks on an object.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnPointerClick(PointerEventData eventData) {
        BarManager.moddedBar = true;
    }
}
