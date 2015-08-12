using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Guides the Line Renderer to point at a specific object.
/// </summary>
[ExecuteInEditMode]
public class HelpCallout : MonoBehaviour {
    /// <summary>
    /// The LineRenderer Component
    /// </summary>
    private LineRenderer lr;
    /// <summary>
    /// The target to point at
    /// </summary>
    public Transform target;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(lr == null) lr = GetComponent<LineRenderer>();

        lr.useWorldSpace = true;
        lr.SetPosition(0, transform.position);  // Start where the owning GameObject is
        lr.SetPosition(1, new Vector3(target.position.x, target.position.y, transform.position.z)); // End where the target is
    }
}
