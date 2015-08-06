using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class FnDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler {
    public static Camera cam;
    public AdvFunction myFunc;
    public Transform LabelParent;
    [System.NonSerialized]
    public GameObject dragItem;
    public static FnDrag draggedItem;

    /// <summary>
    /// Called immediately when the Component's GameObject is enabled
    /// </summary>
    void OnEnable() {
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
                t = "Takedown / Work Lights";
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
            case AdvFunction.PRIO1:
                t = "Priority 1";
                break;
            case AdvFunction.PRIO2:
                t = "Priority 2";
                break;
            case AdvFunction.PRIO3:
                t = "Priority 3";
                break;
            case AdvFunction.PRIO4:
                t = "Priority 4";
                break;
            case AdvFunction.PRIO5:
                t = "Priority 5";
                break;
            case AdvFunction.FTAKEDOWN:
                t = "Flashing Pursuit";
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

    /// <summary>
    /// Called immediately when the Component's GameObject is disable
    /// </summary>
    void OnDisable() {
        dragItem.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData) {
        draggedItem = this;
        dragItem.SetActive(true);

        Vector3 newPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(transform as RectTransform, Input.mousePosition, cam, out newPos);
        dragItem.transform.position = newPos;

        ErrorLogging.LogInput("Began Dragging Function 0x" + ((int)myFunc).ToString("X") + " from sidebar");
    }

    public void OnDrag(PointerEventData eventData) {
        Vector3 newPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(transform as RectTransform, Input.mousePosition, cam, out newPos);
        dragItem.transform.position = newPos;
    }

    public void OnEndDrag(PointerEventData eventData) {
        draggedItem = null;
        dragItem.SetActive(false);

        ErrorLogging.LogInput("Released Function");
    }

    public void OnDrop(PointerEventData eventData) {
        if(FnDragTarget.draggedItem != null) {
            ErrorLogging.LogInput("Removed Function 0x" + FnDragTarget.inputMap.Value[FnDragTarget.draggedItem.key].ToString("X") + " from " + FnDragTarget.draggedItem.key);

            FnDragTarget.inputMap.Value[FnDragTarget.draggedItem.key] = 0;
            BarManager.moddedBar = true;
            if(BarManager.inst.patts.Contains("prog")) BarManager.inst.patts.Remove("prog");
        }
    }
}
