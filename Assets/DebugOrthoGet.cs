using UnityEngine;
using System.Collections;

public class DebugOrthoGet : MonoBehaviour {
    private Camera cam;
    public bool fixing;
    public Vector2 prevSize = Vector2.zero;

    public Transform tl, br;
    public float threshold = 20f;

    void Start() {
        Application.targetFrameRate = 120;

        cam = GetComponent<Camera>();
        fixing = true;
        prevSize = Vector2.zero;
    }

    void Update() {
        Vector2 currSize = new Vector2(Screen.width, Screen.height);
        fixing = true;

        Vector3 tlPoint = cam.WorldToScreenPoint(tl.position), brPoint = cam.WorldToScreenPoint(br.position);

        if(tlPoint.x < 0 || tlPoint.y > cam.pixelHeight || brPoint.x > cam.pixelWidth || brPoint.y < 0) {
            cam.orthographicSize += 0.005f;
        } else if(tlPoint.x > threshold && tlPoint.y < (cam.pixelHeight - threshold) && brPoint.x < (cam.pixelWidth - threshold) && brPoint.y > threshold) {
            cam.orthographicSize -= 0.005f;
        } else {
            fixing = false;
        }
        
    }

    void OnGUI() {
        GUILayout.Box("OS=" + cam.orthographicSize.ToString("0.000") + (fixing ? "...?" : ""));
        float aspRatio = ((Screen.width * 1f) / (Screen.height * 1f));
        if(aspRatio > 3.97f) {
            GUILayout.Box("COS=1.985*");
        } else {
            GUILayout.Box("COS=" + (7.86225f * Mathf.Pow(aspRatio, -0.99787f)));
        }
        GUILayout.Box("Size=" + Screen.width + "w*" + Screen.height + "h");
        if(GUILayout.Button("Save")) {
            DebugOrthoPlot dop = FindObjectOfType<DebugOrthoPlot>();
            dop.points.Add(new DebugOrthoPlot.PlotPoint() { orthoSize = cam.orthographicSize, sHeight = Screen.height, sWidth = Screen.width });
            dop.Refresh();
        }
    }
}
