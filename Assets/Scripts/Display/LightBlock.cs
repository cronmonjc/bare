using UnityEngine;
using System.Collections;

public class LightBlock : MonoBehaviour {
    private static CameraControl cam;
    private MeshRenderer mr;
    private LightHead myHead;
    private Color c;
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

            return gameObject.activeInHierarchy && cam.Selected.Contains(this);
        }
        set {
            if(value && !Selected) {
                cam.Selected.Add(this);
            } else if(!value && Selected) {
                cam.Selected.Remove(this);
            }
        }
    }

    void Start() {
        for(Transform t = transform; myHead == null && t != null; t = t.parent) {
            myHead = t.GetComponent<LightHead>();
        }

        mr = GetComponent<MeshRenderer>();

        c = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        RenderColor = c;
    }

    void Update() {
        RenderColor = c + (Color.white * (Selected ? 0.25f : 0.0f));
    }

    public void Select() {

    }
}
