using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class DisableEnablePatt : MonoBehaviour, IPointerClickHandler {
    public bool IsEnable;

    public void OnPointerClick(PointerEventData eventData) {
        FindObjectOfType<PattSelect>().DisableEnable(IsEnable);
    }
}
