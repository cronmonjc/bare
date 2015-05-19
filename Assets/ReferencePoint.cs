using UnityEngine;
using System.Collections;

public class ReferencePoint : MonoBehaviour {
    void Start() {
        Debug.Log(this.GetPath() + " - " + transform.position);
    }
}
