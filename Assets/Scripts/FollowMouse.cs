using UnityEngine;
using System.Collections;

public class FollowMouse : MonoBehaviour {
    public Camera referenceCamera;
    private CanvasGroup group;
    public Vector2 lastMousePos;
    public Vector2 mouseDelta;

    public static bool BlockMouseInput = false;
    public static bool AppHasFocus = true;

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

    void OnApplicationFocus(bool focus) {
        AppHasFocus = focus;
    }
}
