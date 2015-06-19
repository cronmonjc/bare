using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ClearButton : MonoBehaviour, IPointerClickHandler {

    public void OnPointerClick(PointerEventData eventData) {
        BarManager.moddedBar = true;
    }
}
