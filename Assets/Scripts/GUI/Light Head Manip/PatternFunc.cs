using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class PatternFunc : MonoBehaviour, IPointerClickHandler {
    public FnSelManager fsl;
    public Function fn;

    public void OnPointerClick(PointerEventData eventData) {
        fsl.OnSelect(fn);
    }
}
