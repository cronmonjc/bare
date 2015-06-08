using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CollapsingMenuControl : MonoBehaviour, ILayoutElement {

    public static Camera cam;
    public bool display = false;

    void Start() {
        transform.localScale = new Vector3(1, 0, 1);
    }

    void Update() {
        if(cam == null) {
            cam = GameObject.Find("UI").GetComponent<Camera>();
        }
        if(display && Input.GetMouseButtonDown(0)) {
            bool hide = true;

            if(RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, Input.mousePosition, cam)) {
                hide = false;
            } else {
                foreach(Transform t in GetComponentsInChildren<Transform>()) {
                    if(RectTransformUtility.RectangleContainsScreenPoint(t as RectTransform, Input.mousePosition, cam)) {
                        hide = false;
                    }
                }
            }

            display = !hide;
        }

        float height = transform.localScale.y;
        if((display && height < 1) || (!display && height > 0)) {
            height = Mathf.Lerp(height, display ? 1 : 0, Time.deltaTime * 10.0f); // Tween towards desired result
            if(display) {
                if((1 - height) < 0.05f) {
                    height = 1;
                }

                if(GetComponent<Image>() != null)
                    GetComponent<Image>().enabled = true; // Turn on the menu background
                foreach(Transform child in transform) { // Make all children (aka the menu items) active - rendered and interactible
                    child.gameObject.SetActive(true);
                }
            } else {
                if((height) < 0.05f) {
                    height = 0;

                    if(GetComponent<Image>() != null)
                        GetComponent<Image>().enabled = false; // Turn off the menu background
                    foreach(Transform child in transform) { // Make all children (aka the menu items) inactive
                        child.gameObject.SetActive(false);
                    }
                }
            }
            transform.localScale = new Vector3(1, height, 1);
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
        }
    }

    public void Show() {
        display = true;
    }

    public void ToggleDisplay() {
        display = !display;
    }

    public void CalculateLayoutInputHorizontal() {

    }

    public void CalculateLayoutInputVertical() {

    }

    public float flexibleHeight {
        get { return 0; }
    }

    public float flexibleWidth {
        get { return 0; }
    }

    public int layoutPriority {
        get { return 10; }
    }

    public float minHeight {
        get { return preferredHeight; }
    }

    public float minWidth {
        get { return preferredWidth; }
    }

    public float preferredHeight {
        get { return GetComponent<LayoutGroup>().preferredHeight * transform.localScale.y; }
    }

    public float preferredWidth {
        get { return -1; }
    }
}
