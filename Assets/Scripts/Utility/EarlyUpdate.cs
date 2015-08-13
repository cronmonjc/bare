using UnityEngine;
using System.Collections;

/// <summary>
/// Simple Component, called before all others, to provide the "EarlyUpdate" method.
/// </summary>
public class EarlyUpdate : MonoBehaviour {

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        BroadcastMessage("EarlyUpdate", SendMessageOptions.DontRequireReceiver);
    }
}
