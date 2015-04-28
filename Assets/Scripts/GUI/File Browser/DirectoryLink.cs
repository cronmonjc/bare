using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class DirectoryLink : Selectable, IPointerClickHandler {
    public abstract void Navigate(FileBrowser fb);

    public void OnPointerClick(PointerEventData eventData) {
        Navigate(GameObject.FindObjectOfType<FileBrowser>());
    }
}
