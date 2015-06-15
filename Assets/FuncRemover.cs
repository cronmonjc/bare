using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class FuncRemover : MonoBehaviour, IDropHandler {
    public void OnDrop(PointerEventData eventData) {
        if(FnDragTarget.draggedItem != null) {
            FnDragTarget.inputMap.Value[FnDragTarget.draggedItem.key] = 0;
        }
    }
}
