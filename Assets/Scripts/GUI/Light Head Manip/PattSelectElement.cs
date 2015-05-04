using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class PattSelectElement : MonoBehaviour, IPointerClickHandler {
    public Image image;

    public int selID;
    public PattSelect ps;


    public void OnPointerClick(PointerEventData eventData) {
        ps.Select(this);
    }
}
