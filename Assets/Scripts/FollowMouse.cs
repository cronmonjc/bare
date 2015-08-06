using UnityEngine;
using System.Collections;

/// <summary>
/// A small Component that will prevent the mouse from interacting with the Canvas.  In theory.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(UnityEngine.UI.Image))]
public class FollowMouse : MonoBehaviour {
    public Camera referenceCamera;
    private CanvasGroup group;
    public Vector2 lastMousePos;
    public Vector2 mouseDelta;

    public static bool BlockMouseInput = false;
    public static bool AppHasFocus = true;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        Vector2 currMousePos = Input.mousePosition;

        Vector3 newpos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(((RectTransform)this.transform), currMousePos, referenceCamera, out newpos);
        transform.position = newpos;

        mouseDelta += new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        if((Vector3)lastMousePos != Input.mousePosition) {
            mouseDelta = Vector2.zero;

            lastMousePos = Input.mousePosition;
        }

        if(group == null) group = GetComponent<CanvasGroup>();
        group.blocksRaycasts = BlockMouseInput = !(AppHasFocus && (mouseDelta.sqrMagnitude < 0.15f));
    }

    /// <summary>
    /// OnApplicationFocus is called every time the application gets or loses focus.  (Currently seems to call it twice in quick succession - lost focus, got focus)
    /// </summary>
    /// <param name="focus">Did the application get focus with this call?</param>
    void OnApplicationFocus(bool focus) {
        AppHasFocus = focus;
    }
}
