using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CameraControl : MonoBehaviour {
    public static Function funcBeingTested = Function.NONE;

    public Transform pivot;
    public Vector2 dragStart, currMouse;
    private bool dragging;

    public OpticSelect os;
    public FnSelManager fsm;

    public RectTransform cover;

    public RectTransform SelBox;
    private SelBoxCollider sbc;

    private Camera myCam;

    public GameObject LabelPrefab;
    public Transform LabelParent;

    public GameObject FuncSelectRoot;

    public GameObject FBrowser;

    private List<LightHead> selected;

    public List<LightHead> Selected {
        get {
            if(dragging) {
                return sbc.Selected;
            } else {
                return this.selected;
            }
        }
    }

    public List<LightHead> OnlyCamSelected {
        get {
            return this.selected;
        }
    }

    void Awake() {
        selected = new List<LightHead>();
    }

    void Start() {
        Application.targetFrameRate = 120;

        sbc = SelBox.GetComponent<SelBoxCollider>();

        myCam = GetComponent<Camera>();
        myCam.pixelRect = new Rect(0, Screen.height * 0.45f, Screen.width, Screen.height * 0.55f - 32f);
    }

    void Update() {
        if(!FBrowser.activeInHierarchy) {
            Vector2 mousePos = Input.mousePosition;
            if(Input.GetMouseButtonDown(0) && (funcBeingTested == Function.NONE)) { // LMB pressed
                if(myCam.pixelRect.Contains(mousePos)) {
                    dragging = RectTransformUtility.ScreenPointToLocalPointInRectangle(((RectTransform)SelBox.parent), Input.mousePosition, this.myCam, out dragStart);
                    os.Clear();
                }
            } else if(Input.GetMouseButtonUp(0) && (funcBeingTested == Function.NONE)) { // LMB released
                if(dragging && myCam.pixelRect.Contains(mousePos)) {
                    if(!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
                        selected.Clear();
                    }

                    if(dragging) {
                        if(Selected.Count == 0) { // There isn't anything in the selection box.  Test if there was a light under the cursor?
                            RaycastHit hit;
                            if(Physics.Raycast(myCam.ScreenPointToRay(Input.mousePosition), out hit)) { // There WAS something!
                                LightHead head = hit.transform.GetComponent<LightHead>();
                                if(head != null) {
                                    selected.Add(head);
                                }
                            }
                        } else {
                            foreach(LightHead alpha in sbc.Selected) {
                                if(!selected.Contains(alpha)) {
                                    selected.Add(alpha);
                                }
                            }
                        }

                        sbc.Selected.Clear();
                        SelBox.gameObject.SetActive(false);
                        dragging = false;

                        if(selected.Count > 0) {
                            os.Refresh();
                            fsm.Refresh();
                        }
                    }
                }
            } else if(dragging) { // LMB held
                bool hasHit = RectTransformUtility.ScreenPointToLocalPointInRectangle(((RectTransform)SelBox.parent), Input.mousePosition, this.myCam, out currMouse);
                if(hasHit) {
                    SelBox.gameObject.SetActive(true);

                    Vector3 positioning = new Vector3(0, 0, -0.01f);
                    Vector2 size = new Vector2();
                    if(currMouse.x < dragStart.x) {
                        positioning.x = currMouse.x;
                        size.x = dragStart.x - currMouse.x;
                    } else {
                        positioning.x = dragStart.x;
                        size.x = currMouse.x - dragStart.x;
                    }

                    if(currMouse.y < dragStart.y) {
                        positioning.y = dragStart.y;
                        size.y = dragStart.y - currMouse.y;
                    } else {
                        positioning.y = currMouse.y;
                        size.y = currMouse.y - dragStart.y;
                    }

                    SelBox.localPosition = positioning;
                    SelBox.sizeDelta = size;
                } else {
                    sbc.Selected.Clear();
                    SelBox.gameObject.SetActive(false);
                }
            }

            if((myCam.pixelRect.Contains(mousePos)) && Mathf.Abs(Input.GetAxisRaw("Mouse ScrollWheel")) > 0) {
                myCam.fieldOfView = Mathf.Clamp(myCam.fieldOfView + Input.GetAxisRaw("Mouse ScrollWheel") * 20f, 10, 90f);
            }
        }
    }
}
