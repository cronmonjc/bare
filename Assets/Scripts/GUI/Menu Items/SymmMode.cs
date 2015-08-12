using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Exposes whether or not Symmetry Mode is on from a Toggle Component on the same GameObject
/// </summary>
public class SymmMode : MonoBehaviour {
    /// <summary>
    /// Is Symmetry Mode is on?
    /// </summary>
    public bool On {
        get { return GetComponent<UnityEngine.UI.Toggle>().isOn; }
    }
}
