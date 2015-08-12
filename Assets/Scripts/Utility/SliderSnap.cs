using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI Component.  Snaps the value of the slider to whole numbers.  Should be used only on a slider that doesn't have "whole values only" set.
/// </summary>
[ExecuteInEditMode]
public class SliderSnap : MonoBehaviour, IPointerUpHandler {

    /// <summary>
    /// The callback for what should happen when the whole value changes
    /// </summary>
    public Slider.SliderEvent OnWholeValueChange;

    /// <summary>
    /// The reference to the Slider Component
    /// </summary>
    private Slider s;

    /// <summary>
    /// The last whole value recorded
    /// </summary>
    public int lastWholeVal;

    /// <summary>
    /// Snaps the slider to the nearest whole value.
    /// </summary>
    public void Snap() {
        if(s == null) s = GetComponent<Slider>();

        s.value = Mathf.RoundToInt(s.value);
    }

    /// <summary>
    /// Called when the user releases a mouse button over an object.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnPointerUp(PointerEventData eventData) {
        Snap();
    }

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        if(s == null) s = GetComponent<Slider>();
        lastWholeVal = Mathf.RoundToInt(s.value);
    }


    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        int val = Mathf.RoundToInt(s.value);
        if(val != lastWholeVal) {
            OnWholeValueChange.Invoke(s.value);
            lastWholeVal = val;
        }
    }
}
