using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class FnDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public static Camera cam;
    public AdvFunction myFunc;
    public Transform LabelParent;
    [System.NonSerialized]
    public GameObject dragItem;
    public static FnDrag draggedItem;

    public void OnEnable() {
        if(dragItem != null) return;

        dragItem = new GameObject(gameObject.name + " Drag");
        CanvasGroup cg = dragItem.AddComponent<CanvasGroup>();
        cg.ignoreParentGroups = false;
        cg.blocksRaycasts = false;
        cg.interactable = true;
        dragItem.AddComponent<CanvasRenderer>();
        Image i = dragItem.AddComponent<Image>();
        i.sprite = GetComponent<Image>().sprite;
        i.type = Image.Type.Sliced;

        GameObject dragLabel = new GameObject("Text");
        string t = "";
        switch(myFunc) {
            case AdvFunction.TAKEDOWN:
                t = "Takedown";
                break;
            case AdvFunction.ALLEY_LEFT:
                t = "Left Alley";
                break;
            case AdvFunction.ALLEY_RIGHT:
                t = "Right Alley";
                break;
            case AdvFunction.TURN_LEFT:
                t = "Turn Left";
                break;
            case AdvFunction.TURN_RIGHT:
                t = "Turn Right";
                break;
            case AdvFunction.TAIL:
                t = "Brake Lights";
                break;
            case AdvFunction.T13:
                t = "California T13 Steady";
                break;
            case AdvFunction.EMITTER:
                t = "Emitter";
                break;
            case AdvFunction.LEVEL1:
                t = "Level 1";
                break;
            case AdvFunction.LEVEL2:
                t = "Level 2";
                break;
            case AdvFunction.LEVEL3:
                t = "Level 3";
                break;
            case AdvFunction.LEVEL4:
                t = "Level 4";
                break;
            case AdvFunction.LEVEL5:
                t = "Level 5";
                break;
            case AdvFunction.FTAKEDOWN:
                t = "Flashing Takedown";
                break;
            case AdvFunction.FALLEY:
                t = "Flashing Alley";
                break;
            case AdvFunction.ICL:
                t = "ICL";
                break;
            case AdvFunction.TRAFFIC_LEFT:
                t = "Direct Left";
                break;
            case AdvFunction.TRAFFIC_RIGHT:
                t = "Direct Right";
                break;
            case AdvFunction.CRUISE:
                t = "Cruise";
                break;
            case AdvFunction.DIM:
                t = "Dimmer";
                break;
            case AdvFunction.PATTERN:
                t = "Pattern";
                break;
            default:
                t = "???";
                break;
        }
        Text text = dragLabel.AddComponent<Text>();
        text.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        text.text = t;
        text.color = Color.black;
        dragLabel.transform.SetParent(dragItem.transform);
        dragLabel.transform.localScale = Vector3.one;

        dragItem.AddComponent<HorizontalLayoutGroup>().padding = new RectOffset(4, 4, 4, 4);
        ContentSizeFitter csf = dragItem.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        dragItem.transform.SetParent(LabelParent);
        dragItem.transform.localScale = Vector3.one;

        if(cam == null) cam = GameObject.Find("UI").GetComponent<Camera>();

        dragItem.SetActive(false);
    }

    public void OnDisable() {
        dragItem.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData) {
        draggedItem = this;
        dragItem.SetActive(true);

        Vector3 newPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(transform as RectTransform, Input.mousePosition, cam, out newPos);
        dragItem.transform.position = newPos;
    }

    public void OnDrag(PointerEventData eventData) {
        Vector3 newPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(transform as RectTransform, Input.mousePosition, cam, out newPos);
        dragItem.transform.position = newPos;
    }

    public void OnEndDrag(PointerEventData eventData) {
        draggedItem = null;
        dragItem.SetActive(false);
    }
}
