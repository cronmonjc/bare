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

    /// <summary>
    /// Called immediately when the Component's GameObject is enabled
    /// </summary>
    void OnEnable() {
        GetComponent<UnityEngine.UI.Slider>().value = timescale = 1.0f;
    }

    public void Set(float to) {
        timescale = to;
    }

    /// <summary>
    /// Called immediately when the Component's GameObject is disable
    /// </summary>
    void OnDisable() {
        GetComponent<UnityEngine.UI.Slider>().value = timescale = 1.0f;
    }
}
