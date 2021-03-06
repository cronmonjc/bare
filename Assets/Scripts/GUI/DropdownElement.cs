﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// UI Component.  Deprecated.  Used to be an element for DropdownControls.
/// </summary>
public class DropdownElement : MonoBehaviour, IPointerClickHandler {
    public Text display;
    public int item;

    public void Create(string s, int i) {
        item = i;
        display.text = s;
    }

    public void OnPointerClick(PointerEventData eventData) {
        DropdownControl dc = transform.parent.parent.GetComponent<DropdownControl>();
        if(dc != null) {
            dc.Select(this);
        }
    }
}
