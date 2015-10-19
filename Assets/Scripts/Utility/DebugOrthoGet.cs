using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Debugging.  Displays information about the orthographic size of the camera.
/// </summary>
public class DebugOrthoGet : MonoBehaviour {
    /// <summary>
    /// The reference to the Camera Component
    /// </summary>
    private Camera cam;
    /// <summary>
    /// Is the Component adjusting the Camera's orthographicSize right now?
    /// </summary>
    public bool fixing;

    /// <summary>
    /// The top-left corner of the reference.  Set via Unity Inspector.
    /// </summary>
    public Transform tl;
    /// <summary>
    /// The bottom-right corner of the reference.  Set via Unity Inspector.
    /// </summary>
    public Transform br;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        Application.targetFrameRate = 120;

        cam = GetComponent<Camera>();
        fixing = true;
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {

        float aspRatio = ((Screen.width * 1f) / (Screen.height * 1f));
        cam.orthographicSize = (aspRatio > 5.17f ? 1.44f : (7.70737f * Mathf.Pow(aspRatio, -1.02095f)));

        //if (aspRatio > 5.17f) cam.orthographicSize = 1.44f;
        //else {
        //    Vector3 tlVec = cam.WorldToScreenPoint(tl.position);

        //    if (tlVec.x < 4) cam.orthographicSize += 0.01f;
        //    else if (tlVec.x < 5.25) cam.orthographicSize += 0.001f;
        //    else if (tlVec.x > 7) cam.orthographicSize -= 0.01f;
        //    else if (tlVec.x > 5.75) cam.orthographicSize -= 0.001f;
        //}

        
    }

    /// <summary>
    /// Called when Unity is requesting that GameObjects render GUI elements.  Legacy, avoid use when possible unless for debugging.
    /// </summary>
    void OnGUI() {
        GUILayout.Box("OS=" + cam.orthographicSize.ToString("0.000") + (fixing ? "...?" : ""));
        float aspRatio = ((Screen.width * 1f) / (Screen.height * 1f));
        if(aspRatio > 5.17f) {
            GUILayout.Box("COS=1.44*");
        } else {
            GUILayout.Box("COS=" + (7.70737f * Mathf.Pow(aspRatio, -1.02095f)));
        }
        GUILayout.Box("Size=" + Screen.width + "w*" + Screen.height + "h - AR" + aspRatio.ToString("0.000"));

        Vector3 tlPoint = cam.WorldToScreenPoint(tl.position), brPoint = cam.WorldToScreenPoint(br.position);
        GUILayout.Box("tl=" + tlPoint);
        GUILayout.Box("br=" + brPoint);
    }
}
