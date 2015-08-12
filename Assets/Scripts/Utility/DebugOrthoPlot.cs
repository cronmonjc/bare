using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Debugging.  Creates GameObjects to plot orthographic size in reference to screen width and screen height.
/// </summary>
public class DebugOrthoPlot : MonoBehaviour {
    /// <summary>
    /// A plot point.
    /// </summary>
    [System.Serializable]
    public struct PlotPoint {
        /// <summary>
        /// The screen width of this plot point
        /// </summary>
        public int sWidth;
        /// <summary>
        /// The screen height of this plot point
        /// </summary>
        public int sHeight;
        /// <summary>
        /// The orthographic size of this plot point
        /// </summary>
        public float orthoSize;

        /// <summary>
        /// Performs an implicit conversion from <see cref="PlotPoint"/> to <see cref="Vector3"/>.
        /// </summary>
        public static implicit operator Vector3(PlotPoint pp) {
            return new Vector3(pp.sWidth, pp.orthoSize, pp.sHeight);
        }
    }

    /// <summary>
    /// A concatenation of all of the data of plot points.
    /// </summary>
    [Multiline]
    public string concat;

    /// <summary>
    /// The points themselves
    /// </summary>
    public List<PlotPoint> points;

    /// <summary>
    /// Refreshes this Component.
    /// </summary>
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
