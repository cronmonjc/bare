﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using fNbt;

/// <summary>
/// UI Component.  Acts as a target to drop the functions being dragged from FnDrag.
/// </summary>
public class FnDragTarget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler {
    /// <summary>
    /// The display of what function's currently assigned to this Component.  Set via Unity Inspector.
    /// </summary>
    public Text display;
    /// <summary>
    /// The index of the position this Component's modifying in the input map.  Set via Unity Inspector.
    /// </summary>
    public int key;
    /// <summary>
    /// The shared input map
    /// </summary>
    public static NbtIntArray inputMap;
    /// <summary>
    /// The FnDragTarget currently being dragged
    /// </summary>
    public static FnDragTarget draggedItem;
    /// <summary>
    /// This Component's visually dragged item.  Set via Unity Inspector.
    /// </summary>
    public GameObject dragItem;

    /// <summary>
    /// Reference to a pencil with which to edit the currently assigned function.  Set via Unity Inspector.
    /// </summary>
    public GameObject Edit1, Edit2;

    /// <summary>
    /// Called when the user begins dragging an object just before a drag is started.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnBeginDrag(PointerEventData eventData) {
        if(inputMap.Value[key] == 0) return;

        draggedItem = this;
        dragItem.gameObject.SetActive(true);

        ErrorLogging.LogInput("Began Dragging Function 0x" + ((int)inputMap.Value[key]).ToString("X") + " from " + key);
    }

    /// <summary>
    /// When dragging is occurring, this will be called every time the cursor is moved.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnDrag(PointerEventData eventData) {
        if(inputMap.Value[key] == 0) return;

        Vector3 newPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(transform as RectTransform, Input.mousePosition, FnDrag.cam, out newPos);
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
        #region Assigning a Function
        if(FnDrag.draggedItem != null) {
            int newFunc = (int)FnDrag.draggedItem.myFunc;

            int[] val = inputMap.Value;
            if((val[key] | newFunc) == (int)(AdvFunction.FALLEY | AdvFunction.FTAKEDOWN) &&
                (val[key] != (int)(AdvFunction.FALLEY | AdvFunction.FTAKEDOWN))) {
                val[key] = (int)(AdvFunction.FALLEY | AdvFunction.FTAKEDOWN);

                ErrorLogging.LogInput("Assigned Function 0xC00 to " + key);
            } else {
                val[key] = newFunc;

                ErrorLogging.LogInput("Assigned Function 0x" + newFunc.ToString("X") + " to " + key);
            }

            for(int i = 0; i < 20; i++) {
                if(key == i) continue;
                if((val[i] & newFunc) > 0) {
                    val[i] &= ~newFunc;
                }
            }
            inputMap.Value = val;
            BarManager.moddedBar = true;
            if(BarManager.inst.patts.Contains("prog")) BarManager.inst.patts.Remove("prog");
        #endregion
        #region Moving a Function
        } else if(FnDragTarget.draggedItem != null) {
            int[] val = inputMap.Value;
            int newFunc = val[FnDragTarget.draggedItem.key];

            val[FnDragTarget.draggedItem.key] = 0;
            if((val[key] | newFunc) == (int)(AdvFunction.FALLEY | AdvFunction.FTAKEDOWN) &&
                (val[key] != (int)(AdvFunction.FALLEY | AdvFunction.FTAKEDOWN))) {
                val[key] = (int)(AdvFunction.FALLEY | AdvFunction.FTAKEDOWN);

                ErrorLogging.LogInput("Moved Function 0x" + newFunc.ToString("X") + " from " + FnDragTarget.draggedItem.key + " to " + key + " - merged");
            } else {
                val[key] = newFunc;

                ErrorLogging.LogInput("Moved Function 0x" + newFunc.ToString("X") + " from " + FnDragTarget.draggedItem.key + " to " + key);
            }
            inputMap.Value = val;
            BarManager.moddedBar = true;
            if(BarManager.inst.patts.Contains("prog")) BarManager.inst.patts.Remove("prog");
        }
        #endregion
    }

    /// <summary>
    /// Clears the mapping of this Component.
    /// </summary>
    public void Clear() {
        int[] val = inputMap.Value;
        val[key] = 0;
        inputMap.Value = val;
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        int val = inputMap.Value[key];

        #region Show pencils when necessary
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
        #endregion

        #region Show which function's assigned
        switch(val) {
            case 0x1: // TAKEDOWN
                display.text = "Takedown / Work Lights";
                break;
            case 0x2: // PRIO1
                display.text = "Priority 1";
                break;
            case 0x4: // PRIO2
                display.text = "Priority 2";
                break;
            case 0x8: // PRIO3
                display.text = "Priority 3";
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
                display.text = "Flashing Pursuit";
                break;
            case 0x800: // FALLEY
                display.text = "Flashing Alley";
                break;
            case 0xC00: // FTAKEDOWN | FALLEY
                display.text = "Flashing Alley & Pursuit";
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
                display.text = "Tail Lights";
                break;
            case 0x20000: // T13
                display.text = "California T13 Steady";
                break;
            case 0x40000: // PRIO4
                display.text = "Priority 4";
                break;
            case 0x80000: // PRIO5
                display.text = "Priority 5";
                break;
            case 0x100000: // EMITTER
                display.text = "Emitter";
                break;
            default:
                display.text = "";
                break;
        } 
        #endregion

        #region Colorize display text indicating whether or not it's in use
        short en = 0x0;
        NbtCompound patts = BarManager.inst.patts;
        switch(val) {
            case 0xC00:
                NbtCompound afl = patts.Get<NbtCompound>("afl"), tdp = patts.Get<NbtCompound>("tdp");
                foreach(string alpha in new string[] { "ef1", "ef2", "er1", "er2" }) {
                    en |= afl[alpha].ShortValue;
                    en |= tdp[alpha].ShortValue;
                }
                break;
            case 0x10:
            case 0x20:
                NbtCompound traf = patts.Get<NbtCompound>("traf");
                foreach(string alpha in new string[] { "er1", "er2" }) {
                    en |= traf[alpha].ShortValue;
                }
                break;
            case 0x0:
            case 0x1000:
                en = 1;
                break;
            default:
                NbtCompound cmpd = patts.Get<NbtCompound>(BarManager.GetFnString(BarManager.inst.transform, (AdvFunction)val));
                foreach(string alpha in new string[] { "ef1", "ef2", "er1", "er2" }) {
                    en |= cmpd[alpha].ShortValue;
                }
                break;

        }
        display.color = (en == 0 ? new Color(0.75f, 0.75f, 0.75f, 1.0f) : new Color(0.2f, 0.2f, 0.2f, 1.0f)); 
        #endregion
    }
}
