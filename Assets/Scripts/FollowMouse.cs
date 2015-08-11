using UnityEngine;
using System.Collections;

/// <summary>
/// A small Component that will prevent the mouse from interacting with the Canvas.  In theory.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(UnityEngine.UI.Image))]
public class FollowMouse : MonoBehaviour {
    /// <summary>
    /// The Camera Component against which to reference the mouse's position.  Set by the Unity Inspector.
    /// </summary>
    public Camera referenceCamera;
    /// <summary>
    /// A reference to the CanvasGroup Conponent used to block UI input
    /// </summary>
    private CanvasGroup group;
    /// <summary>
    /// The last position the mouse was at
    /// </summary>
    public Vector2 lastMousePos;
    /// <summary>
    /// The distance the mouse has moved from the last position
    /// </summary>
    public Vector2 mouseDelta;

    /// <summary>
    /// Whether or not mouse input should be blocked
    /// </summary>
    public static bool BlockMouseInput = false;
    /// <summary>
    /// Whether or not the application has keyboard / mouse focus
    /// </summary>
    public static bool AppHasFocus = true;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        Vector2 currMousePos = Input.mousePosition;

        Vector3 newpos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(((RectTransform)this.transform), currMousePos, referenceCamera, out newpos); // Figure out where the mouse is in world
        transform.position = newpos;  // Move blocker GameObject there

        mouseDelta += new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")); // Calculate the delta
        if((Vector3)lastMousePos != Input.mousePosition) {
            mouseDelta = Vector2.zero; // Mouse has moved, zero the delta

            lastMousePos = Input.mousePosition; // Reset last position
        }

        if(group == null) group = GetComponent<CanvasGroup>(); // Get reference to the CanvasGroup Component if we don't have it already
        group.blocksRaycasts = BlockMouseInput = !(AppHasFocus && (mouseDelta.sqrMagnitude < 0.15f)); // Block the mouse if necessary
    }

    /// <summary>
    /// OnApplicationFocus is called every time the application gets or loses focus.  (Currently seems to call it twice in quick succession - lost focus, got focus)
    /// </summary>
    /// <param name="focus">Did the application get focus with this call?</param>
    void OnApplicationFocus(bool focus) {
        AppHasFocus = focus;
    }
}
