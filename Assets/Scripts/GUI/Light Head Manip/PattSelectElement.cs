using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class PattSelectElement : MonoBehaviour, IPointerClickHandler {
    public Image image;

    public short selID;
    public PattSelect ps;

    public string Function {
        set {
            GetComponentInChildren<Text>().text = value;
        }
    }

    public bool PattActive {
        get {
            return image.enabled;
        }
        set {
            image.enabled = value;
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        ps.Select(this);
    }
}
