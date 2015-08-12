using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// UI Component.  Shows and hides the help panel.
/// </summary>
public class Help : MonoBehaviour, IPointerDownHandler {

    /// <summary>
    /// Shows this Component.  Called by clicking the Help Button.
    /// </summary>
    public void Show() {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Called when the user presses a mouse button over an object.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnPointerDown(PointerEventData eventData) {
        gameObject.SetActive(false);  // Click anywhere = hide help.
    }
}
