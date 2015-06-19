using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Help : MonoBehaviour, IPointerDownHandler {

    public void Show() {
        gameObject.SetActive(true);
    }

    public void OnPointerDown(PointerEventData eventData) {
        gameObject.SetActive(false);
    }
}
