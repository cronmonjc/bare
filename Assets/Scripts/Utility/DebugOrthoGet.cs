﻿using UnityEngine;
using System.Collections;

public class DebugOrthoGet : MonoBehaviour {
    private Camera cam;
    public bool fixing;

    public Transform tl, br;
    public float threshold = 20f;

    void Start() {
        Application.targetFrameRate = 120;

        cam = GetComponent<Camera>();
        fixing = true;
    }

    void Update() {

        float aspRatio = ((Screen.width * 1f) / (Screen.height * 1f));
        cam.orthographicSize = (aspRatio > 3.97f ? 1.985f : (7.86225f * Mathf.Pow(aspRatio, -0.99787f)));
        
    }

    void OnGUI() {
        GUILayout.Box("OS=" + cam.orthographicSize.ToString("0.000") + (fixing ? "...?" : ""));
        float aspRatio = ((Screen.width * 1f) / (Screen.height * 1f));
        if(aspRatio > 3.97f) {
            GUILayout.Box("COS=1.985*");
        } else {
            GUILayout.Box("COS=" + (7.86225f * Mathf.Pow(aspRatio, -0.99787f)));
        }
        GUILayout.Box("Size=" + Screen.width + "w*" + Screen.height + "h - AR" + aspRatio.ToString("0.000"));

        Vector3 tlPoint = cam.WorldToScreenPoint(tl.position), brPoint = cam.WorldToScreenPoint(br.position);
        GUILayout.Box("tl=" + tlPoint);
        GUILayout.Box("br=" + brPoint);
    }
}