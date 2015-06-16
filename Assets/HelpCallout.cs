using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class HelpCallout : MonoBehaviour {
    private LineRenderer lr;
    public Transform target;

    void Update() {
        if(lr == null) lr = GetComponent<LineRenderer>();

        lr.useWorldSpace = true;
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, new Vector3(target.position.x, target.position.y, transform.position.z));
    }
}
