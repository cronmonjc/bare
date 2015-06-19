using UnityEngine;
using System.Collections;

public class TimeSlider : MonoBehaviour {
    public float timescale {
        get {
            return Time.timeScale;
        }
        set {
            Time.timeScale = value;
            Time.fixedDeltaTime = 0.02f * value;
        }
    }

    void OnEnable() {
        GetComponent<UnityEngine.UI.Slider>().value = timescale = 1.0f;
    }

    public void Set(float to) {
        timescale = to;
    }

    void OnDisable() {
        GetComponent<UnityEngine.UI.Slider>().value = timescale = 1.0f;
    }
}
