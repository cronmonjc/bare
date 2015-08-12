using UnityEngine;
using System.Collections;

/// <summary>
/// Deprecated.  Used to help position the camera in specific locations to provide specific views of the 3D bar.
/// </summary>
public class CameraPositioning : MonoBehaviour {
    public void HomeView() {
        SetView(new Vector3(-20f, 0f, 0f));
    }

    public void OverheadView() {
        SetView(new Vector3(-10f, 0f, 0f));
    }

    public void FrontView() {
        SetView(new Vector3(-80f, 180f, 0f));
    }

    public void DriverView() {
        SetView(new Vector3(-80f, 90f, 0f));
    }

    public void PassengerView() {
        SetView(new Vector3(-80f, 270f, 0f));
    }

    public void RearView() {
        SetView(new Vector3(-80f, 0f, 0f));
    }

    public void SetView(Vector3 euler) {
        transform.position = Vector3.zero;
        transform.eulerAngles = euler;
    }
}
