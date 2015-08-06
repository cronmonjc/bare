using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Deprecated.  Used to display an individual color for a light head (splitting the head if it was dual color).
/// </summary>
[System.Obsolete("No longer used, as light heads individually maintain what color they should display")]
public class LightBlock : MonoBehaviour {
    private static CameraControl cam;
    private MeshRenderer mr;
    private LightHead myHead;
    private Color c;

    public Dictionary<AdvFunction, Pattern> patterns;

    public Color Color {
        get {
            return c;
        }
        set {
            c = value;
        }
    }

    public Color RenderColor {
        set {
            foreach(Light l in GetComponentsInChildren<Light>(true)) {
                l.color = c;
            }
            mr.material.SetColor("_Color", value);
            mr.material.SetColor("_EmissionColor", value * 0.4f);
        }
    }

    public float Smoothness {
        set {
            mr.material.SetFloat("_Glossiness", value);
        }
    }

    public bool Selected {
        get {
            if(cam == null) {
                cam = FindObjectOfType<CameraControl>();
            }

            return gameObject.activeInHierarchy; //&& cam.Selected.Contains(this);
        }
        set {
            if(value && !Selected) {
                //cam.Selected.Add(this);
            } else if(!value && Selected) {
                //cam.Selected.Remove(this);
            }
        }
    }

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        patterns = new Dictionary<AdvFunction, Pattern>();

        for(Transform t = transform; myHead == null && t != null; t = t.parent) {
            myHead = t.GetComponent<LightHead>();
        }

        mr = GetComponent<MeshRenderer>();

        c = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        RenderColor = c;
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        RenderColor = c + (Color.white * (Selected ? 0.25f : 0.0f));
    }
}
