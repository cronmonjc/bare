using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class DropdownControl : MonoBehaviour {
    private Text display;
    private CollapsingMenuControl cmc;

    public bool IsOpen {
        get { return cmc.display; }
    }

    public GameObject ElementPrefab;
    public int selected = -1;
    public Transform menu;
    public string TextOnRefresh;

    [System.Serializable]
    public class SelectionCallback : UnityEvent<int> { }

    public SelectionCallback OnSelect;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        display = GetComponent<Text>();
        cmc = GetComponentInChildren<CollapsingMenuControl>();
        Clear();
    }

    public void Clear() {
        selected = -1;

        foreach(Transform t in new List<Transform>(menu.GetComponentsInChildren<Transform>())) {
            Destroy(t.gameObject);
        }
    }

    public void Refresh(params string[] items) {
        Clear();

        for(int i = 0; i < items.Length; i++) {
            GameObject newbie = Instantiate<GameObject>(ElementPrefab);
            newbie.transform.SetParent(menu);
            DropdownElement newde = newbie.GetComponent<DropdownElement>();
            newde.Create(items[i], i);
        }

        display.text = TextOnRefresh;
    }

    public void Select(DropdownElement de) {
        selected = de.item;
        display.text = de.display.text;

        OnSelect.Invoke(selected);
    }
}
