using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using fNbt;

public class FnDragTarget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler {
    public Text display;
    public int key;
    public static NbtIntArray inputMap;
    public static FnDragTarget draggedItem;
    public GameObject dragItem;

    public GameObject Edit1, Edit2;

    public void OnBeginDrag(PointerEventData eventData) {
        if(inputMap.Value[key] == 0) return;

        draggedItem = this;
        dragItem.gameObject.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData) {
        if(inputMap.Value[key] == 0) return;

        Vector3 newPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(transform as RectTransform, Input.mousePosition, FnDrag.cam, out newPos);
        dragItem.transform.position = newPos;
    }

    public void OnEndDrag(PointerEventData eventData) {
        draggedItem = null;
        dragItem.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData) {
        if(FnDrag.draggedItem != null) {
            int newFunc = (int)FnDrag.draggedItem.myFunc;
            int[] val = inputMap.Value;
            if((val[key] | newFunc) == (int)(AdvFunction.FALLEY | AdvFunction.FTAKEDOWN) &&
                (val[key] != (int)(AdvFunction.FALLEY | AdvFunction.FTAKEDOWN))) {
                val[key] = (int)(AdvFunction.FALLEY | AdvFunction.FTAKEDOWN);
            } else {
                val[key] = newFunc;
            }

            for(int i = 0; i < 20; i++) {
                if(key == i) continue;
                if((val[i] & newFunc) > 0) {
                    val[i] &= ~newFunc;
                }
            }
        } else if(FnDragTarget.draggedItem != null) {
            int[] val = inputMap.Value;
            int newFunc = val[FnDragTarget.draggedItem.key];
            val[FnDragTarget.draggedItem.key] = 0;
            if((val[key] | newFunc) == (int)(AdvFunction.FALLEY | AdvFunction.FTAKEDOWN) &&
                (val[key] != (int)(AdvFunction.FALLEY | AdvFunction.FTAKEDOWN))) {
                val[key] = (int)(AdvFunction.FALLEY | AdvFunction.FTAKEDOWN);
            } else {
                val[key] = newFunc;
            }
        }
    }

    public void Clear() {
        int[] val = inputMap.Value;
        val[key] = 0;
        inputMap.Value = val;
    }

    public void Update() {
        int val = inputMap.Value[key];

        if(val == 0xC00) {
            Edit1.SetActive(true);
            Edit2.SetActive(true);
        } else if(val == 0x0 || val == 0x1000) {
            Edit1.SetActive(false);
            Edit2.SetActive(false);
        } else {
            Edit1.SetActive(true);
            Edit2.SetActive(false);
        }

        switch(val) {
            case 0x1: // TAKEDOWN
                display.text = "Takedown / Work Lights";
                break;
            case 0x2: // LEVEL1
                display.text = "Level 1";
                break;
            case 0x4: // LEVEL2
                display.text = "Level 2";
                break;
            case 0x8: // LEVEL3
                display.text = "Level 3";
                break;
            case 0x10: // TRAFFIC_LEFT
                display.text = "Direct Left";
                break;
            case 0x20: // TRAFFIC_RIGHT
                display.text = "Direct Right";
                break;
            case 0x40: // ALLEY_LEFT
                display.text = "Left Alley";
                break;
            case 0x80: // ALLEY_RIGHT
                display.text = "Right Alley";
                break;
            case 0x100: // ICL
                display.text = "ICL";
                break;
            case 0x200: // DIM
                display.text = "Dimmer";
                break;
            case 0x400: // FTAKEDOWN
                display.text = "Flashing Takedown";
                break;
            case 0x800: // FALLEY
                display.text = "Flashing Alley";
                break;
            case 0xC00: // FTAKEDOWN | FALLEY
                display.text = "Flashing Alley & Takedown";
                break;
            case 0x1000: // PATTERN
                display.text = "Pattern";
                break;
            case 0x2000: // CRUISE
                display.text = "Cruise";
                break;
            case 0x4000: // TURN_LEFT
                display.text = "Turn Left";
                break;
            case 0x8000: // TURN_RIGHT
                display.text = "Turn Right";
                break;
            case 0x10000: // TAIL
                display.text = "Brake Lights";
                break;
            case 0x20000: // T13
                display.text = "California T13 Steady";
                break;
            case 0x40000: // LEVEL4
                display.text = "Level 4";
                break;
            case 0x80000: // LEVEL5
                display.text = "Level 5";
                break;
            case 0x100000: // EMITTER
                display.text = "Emitter";
                break;
            default:    
                display.text = "";
                break;
        }
    }
}
