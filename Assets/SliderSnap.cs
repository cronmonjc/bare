using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SliderSnap : MonoBehaviour, IPointerUpHandler {

    public Slider.SliderEvent OnWholeValueChange;

    private Slider s;

    public int lastWholeVal;

    public void Snap() {
        if(s == null) s = GetComponent<Slider>();

        s.value = Mathf.RoundToInt(s.value);
    }

    public void OnPointerUp(PointerEventData eventData) {
        Snap();
    }

    void Start() {
        if(s == null) s = GetComponent<Slider>();
        lastWholeVal = Mathf.RoundToInt(s.value);
    }

    
    void Update() {
        int val = Mathf.RoundToInt(s.value);
        if(val != lastWholeVal) {
            OnWholeValueChange.Invoke(s.value);
            lastWholeVal = val;
        }
    }
}
