using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI Component, File Browser Link, abstract.  Will Navigate to a specific location when clicked.
/// </summary>
public abstract class DirectoryLink : Selectable, IPointerClickHandler {
    /// <summary>
    /// Allows the Link to tell the provided File Browser to Navigate to wherever the Link is set to go.
    /// </summary>
    /// <param name="fb">The File Browser to Navigate.</param>
    public abstract void Navigate(FileBrowser fb);

    /// <summary>
    /// Called by Unity when the user clicks on the GameObject containing this Component.
    /// </summary>
    /// <param name="eventData">The event data containing information about the click.</param>
    public void OnPointerClick(PointerEventData eventData) {
        Navigate(GameObject.FindObjectOfType<FileBrowser>());
    }
}
