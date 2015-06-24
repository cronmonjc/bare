using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugOrthoPlot : MonoBehaviour {
    [System.Serializable]
    public struct PlotPoint {
        public int sWidth, sHeight;
        public float orthoSize;

        public static implicit operator Vector3(PlotPoint pp) {
            return new Vector3(pp.sWidth, pp.orthoSize, pp.sHeight);
        }
    }

    [Multiline]
    public string concat;

    public List<PlotPoint> points;

    public void Refresh() {
        foreach(Transform t in transform) {
            Destroy(t.gameObject);
        }

        concat = "";

        foreach(PlotPoint p in points) {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.parent = transform;
            go.transform.localScale = new Vector3(40, 0.04f, 40);
            go.transform.localPosition = p;
            Destroy(go.GetComponent<BoxCollider>());
            concat = concat + ((p.sWidth * 1f) / (p.sHeight * 1f)) + " " + p.orthoSize + "\n";
        }
    }

}
