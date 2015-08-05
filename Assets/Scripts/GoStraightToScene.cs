using UnityEngine;
using System.Collections;

public class GoStraightToScene : MonoBehaviour {
    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        Application.LoadLevel(1);
    }
}
