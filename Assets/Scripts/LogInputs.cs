using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class LogInputs : MonoBehaviour, IPointerClickHandler {
    private string path = "";
    public UnityEngine.UI.Text label;

    /// <summary>
    /// Called immediately when the Component's GameObject is enabled
    /// </summary>
    void OnEnable() {
        if(string.IsNullOrEmpty(path)) path = transform.GetPath();
    }

    //public void OnMouseEnter() {
    //    if(ErrorLogging.allowInputLogging)
    //        ErrorLogging.LogInput("Mouse Enter on " + path + " - " + Input.mousePosition.ToString("F1") + (label != null ? " - " + string.Join(", ", label.text.Split('\n')) : ""));
    //}

    //public void OnMouseExit() {
    //    if(ErrorLogging.allowInputLogging)
    //        ErrorLogging.LogInput("Mouse Exit on " + path + " - " + Input.mousePosition.ToString("F1") + (label != null ? " - " + string.Join(", ", label.text.Split('\n')) : ""));
    //}

    //public void OnMouseDown() {
    //    if(ErrorLogging.allowInputLogging)
    //        ErrorLogging.LogInput("Mouse Down on " + path + " - " + Input.mousePosition.ToString("F1") + (label != null ? " - " + string.Join(", ", label.text.Split('\n')) : ""));
    //}

    /// <summary>
    /// Called when the left mouse button is pressed then released on a Collider on this GameObject
    /// </summary>
    void OnMouseUpAsButton() {
        if(ErrorLogging.allowInputLogging)
            ErrorLogging.LogInput("Mouse Clicked on " + path + " - " + Input.mousePosition.ToString("F1") + (label != null ? " - " + string.Join(", ", label.text.Split('\n')) : ""));
    }

    /// <summary>
    /// Called when a mouse button is pressed then released on a Selectable Component on this GameObject
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        OnMouseUpAsButton();
    }
}