using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// UI Component.  Allows for the dragging of functions from the function list
/// </summary>
public class FnDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler {
    /// <summary>
    /// Reference to the camera creating the drag events
    /// </summary>
    public static Camera cam;
    /// <summary>
    /// The function this Component is responsible for.  Set via Unity Inspector.
    /// </summary>
    public AdvFunction myFunc;
    /// <summary>
    /// The parent Transform upon which to put the label.  Set via Unity Inspector.
    /// </summary>
    public Transform LabelParent;
    /// <summary>
    /// The item that is visibly dragged around
    /// </summary>
    [System.NonSerialized]
    public GameObject dragItem;
    /// <summary>
    /// The FnDrag Component that owns the item currently being dragged
    /// </summary>
    public static FnDrag draggedItem;

    /// <summary>
    /// Called immediately when the Component's GameObject is enabled
    /// </summary>
    void OnEnable() {
        if(dragItem != null) return; // If this Component already owns a drag item, stop here

        #region Create new drag item
        dragItem = new GameObject(gameObject.name + " Drag");
        CanvasGroup cg = dragItem.AddComponent<CanvasGroup>();
        cg.ignoreParentGroups = false;
        cg.blocksRaycasts = false;
        cg.interactable = true;
        dragItem.AddComponent<CanvasRenderer>();
        Image i = dragItem.AddComponent<Image>();
        i.sprite = GetComponent<Image>().sprite;
        i.type = Image.Type.Sliced; 
        #endregion

        #region Create label on drag item showing name of function
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
        #endregion

        #region Add Content Size Fitter
        dragItem.AddComponent<HorizontalLayoutGroup>().padding = new RectOffset(4, 4, 4, 4);
        ContentSizeFitter csf = dragItem.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        dragItem.transform.SetParent(LabelParent);
        dragItem.transform.localScale = Vector3.one; 
        #endregion

        if(cam == null) cam = GameObject.Find("UI").GetComponent<Camera>(); // Get reference to camera if there isn't one already

        dragItem.SetActive(false);
    }

    /// <summary>
    /// Called immediately when the Component's GameObject is disabled
    /// </summary>
    void OnDisable() {
        dragItem.SetActive(false);
    }

    /// <summary>
    /// Called when the user begins dragging an object just before a drag is started.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnBeginDrag(PointerEventData eventData) {
        draggedItem = this;
        dragItem.SetActive(true);

        Vector3 newPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(transform as RectTransform, Input.mousePosition, cam, out newPos);
        dragItem.transform.position = newPos;

        ErrorLogging.LogInput("Began Dragging Function 0x" + ((int)myFunc).ToString("X") + " from sidebar");
    }

    /// <summary>
    /// When dragging is occurring, this will be called every time the cursor is moved.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnDrag(PointerEventData eventData) {
        Vector3 newPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(transform as RectTransform, Input.mousePosition, cam, out newPos);
        dragItem.transform.position = newPos;
    }

    /// <summary>
    /// Called when the user releases a dragged object.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnEndDrag(PointerEventData eventData) {
        draggedItem = null;
        dragItem.SetActive(false);

        ErrorLogging.LogInput("Released Function");
    }

    /// <summary>
    /// Called when the user releases a dragged object on a target that can accept a drop.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnDrop(PointerEventData eventData) {
        if(FnDragTarget.draggedItem != null) { // We were dragging a function from a FnDragTarget
            ErrorLogging.LogInput("Removed Function 0x" + FnDragTarget.inputMap.Value[FnDragTarget.draggedItem.key].ToString("X") + " from " + FnDragTarget.draggedItem.key);

            FnDragTarget.inputMap.Value[FnDragTarget.draggedItem.key] = 0;
            BarManager.moddedBar = true;
            if(BarManager.inst.patts.Contains("prog")) BarManager.inst.patts.Remove("prog");
        }
    }
}
