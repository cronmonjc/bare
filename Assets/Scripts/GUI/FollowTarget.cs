using UnityEngine;
using System.Collections;

/// <summary>
/// A simple script.  Causes the attached GameObject to follow another GameObject.
/// </summary>
public class FollowTarget : MonoBehaviour {
    /// <summary>
    /// The object to follow.
    /// </summary>
    public Transform target;

    // Update is called once per frame
    void Update() {
        transform.position = target.position;
    }
}
