using UnityEngine;
using System.Collections;

public class ReferencePoint : MonoBehaviour {
    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
#if UNITY_EDITOR
        Debug.Log(this.GetPath() + " - " + transform.position);
    }

    /// <summary>
    /// EDITOR ONLY.  Tells Unity how to render this GameObject in the Scene View.
    /// </summary>
    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position, Vector3.one * 0.1f);
        Gizmos.DrawLine(transform.position + new Vector3(-10, 0, 0), transform.position + new Vector3(10, 0, 0));
        Gizmos.DrawLine(transform.position + new Vector3(0, -10, 0), transform.position + new Vector3(0, 10, 0));
        Gizmos.DrawLine(transform.position + new Vector3(0, 0, -10), transform.position + new Vector3(0, 0, 10));
        Gizmos.color = Color.white;
#endif
    }
}
