using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// UI Component.  Animates and hides away the various menu controls.
/// </summary>
public class CollapsingMenuControl : MonoBehaviour, ILayoutElement {
    /// <summary>
    /// The Camera Component viewing this Component
    /// </summary>
    public static Camera cam;
    /// <summary>
    /// Whether or not this Component is displayed
    /// </summary>
    public bool display = false;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        transform.localScale = new Vector3(1, 0, 1);
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(cam == null) {
            cam = GameObject.Find("UI").GetComponent<Camera>();  // Get reference if we don't have it already
        }
        #region Hide the menu if the user clicks off
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
        #endregion

        #region Animate height and show/hide the menu
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
        #endregion
    }

    /// <summary>
    /// Shows this Component.
    /// </summary>
    public void Show() {
        display = true;
    }

    /// <summary>
    /// Toggles the display of this Component.
    /// </summary>
    public void ToggleDisplay() {
        display = !display;
    }

    /// <summary>
    /// The minWidth, preferredWidth, and flexibleWidth values may be calculated in this callback.
    /// </summary>
    public void CalculateLayoutInputHorizontal() {

    }

    /// <summary>
    /// The minHeight, preferredHeight, and flexibleHeight values may be calculated in this callback.
    /// </summary>
    public void CalculateLayoutInputVertical() {

    }

    /// <summary>
    /// The extra relative height this layout element should be allocated if there is additional available space.
    /// </summary>
    public float flexibleHeight {
        get { return 0; }
    }

    /// <summary>
    /// The extra relative width this layout element should be allocated if there is additional available space.
    /// </summary>
    public float flexibleWidth {
        get { return 0; }
    }

    /// <summary>
    /// <para>The layout priority of this component.</para>
    /// <para>If multiple components on the same GameObject implement the ILayoutElement interface, the values provided by components that return a higher priority value are given priority. However, values less than zero are ignored. This way a component can override only select properties by leaving the remaning values to be -1 or other values less than zero.</para>
    /// </summary>
    public int layoutPriority {
        get { return 10; }
    }

    /// <summary>
    /// The minimum height this layout element may be allocated.
    /// </summary>
    public float minHeight {
        get { return preferredHeight; }
    }

    /// <summary>
    /// The minimum width this layout element may be allocated.
    /// </summary>
    public float minWidth {
        get { return preferredWidth; }
    }

    /// <summary>
    /// The preferred height this layout element should be allocated if there is sufficient space.
    /// </summary>
    public float preferredHeight {
        get { return GetComponent<LayoutGroup>().preferredHeight * transform.localScale.y; }
    }

    /// <summary>
    /// The preferred width this layout element should be allocated if there is sufficient space.
    /// </summary>
    public float preferredWidth {
        get { return -1; }
    }
}
