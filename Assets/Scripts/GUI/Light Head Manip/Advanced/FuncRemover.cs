using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// UI Component.  Removes a function from the Input Map when dragged off of it.
/// </summary>
public class FuncRemover : MonoBehaviour, IDropHandler {
    /// <summary>
    /// Called when the user releases a dragged object on a target that can accept a drop.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnDrop(PointerEventData eventData) {
        if(FnDragTarget.draggedItem != null) {
            FnDragTarget.inputMap.Value[FnDragTarget.draggedItem.key] = 0;
            BarManager.moddedBar = true;
            if(BarManager.inst.patts.Contains("prog")) BarManager.inst.patts.Remove("prog");
        }
    }
}
