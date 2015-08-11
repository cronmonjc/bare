using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Sits on the slider that allows for the slowing of the preview.
/// </summary>
public class TimeSlider : MonoBehaviour {
    /// <summary>
    /// Gets or sets the preview timescale.
    /// </summary>
    /// <remarks>
    /// The way it works is by telling Unity to slow down the way it simulates time application-wide.
    /// Reducing Time.timeScale below 1.0 will also reduce Time.deltaTime, which PattTimer depends upon to know the time.
    /// A reduced Time.deltaTime means less recorded second per second, which means longer ticks, which means slower animation.
    /// </remarks>
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

    /// <summary>
    /// Sets the timescale with the specified value.  Called when the slider is moved by the user.
    /// </summary>
    /// <param name="to">To.</param>
    public void Set(float to) {
        timescale = to;
    }

    /// <summary>
    /// Called immediately when the Component's GameObject is disabled
    /// </summary>
    void OnDisable() {
        GetComponent<UnityEngine.UI.Slider>().value = timescale = 1.0f;
    }
}
