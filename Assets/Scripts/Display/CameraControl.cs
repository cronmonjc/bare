using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CameraControl : MonoBehaviour {
    public static AdvFunction funcBeingTested = AdvFunction.NONE;

    public static bool ShowPricing = false;
    public static bool ShowWhole = false;

    public Transform pivot;
    public Vector2 dragStart, currMouse;
    private bool dragging;

    private float partialOrtho = 2.0f;

    public FunctionSelect fs;
    public FnSelManager fsm;

    public RectTransform cover;

    public RectTransform SelBox;
    private SelBoxCollider sbc, msbc;
    private SymmMode sm;

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
        msbc = SelBox.transform.GetChild(0).GetComponent<SelBoxCollider>();

        sm = FindObjectOfType<SymmMode>();

        myCam = GetComponent<Camera>();
        myCam.pixelRect = new Rect(0, Screen.height * 0.45f, Screen.width, Screen.height * 0.55f - 32f);
    }

    void Update() {
        if(!FBrowser.activeInHierarchy) {
            if(ShowWhole) {
                myCam.pixelRect = new Rect(0, 0, Screen.width, Screen.height);
                float aspRatio = (Screen.width * 1.0f) / (Screen.height * 1.0f);
                myCam.orthographicSize = (aspRatio > 3.97f ? 1.985f : (7.86225f * Mathf.Pow(aspRatio, -0.99787f)));
            } else {
                myCam.pixelRect = new Rect(0, Screen.height * 0.45f, Screen.width, Screen.height * 0.55f - 32f);

                Vector2 mousePos = Input.mousePosition;
                if(Input.GetMouseButtonDown(0) && (funcBeingTested == AdvFunction.NONE)) { // LMB pressed
                    if(myCam.pixelRect.Contains(mousePos)) {
                        dragging = RectTransformUtility.ScreenPointToLocalPointInRectangle(((RectTransform)SelBox.parent), Input.mousePosition, this.myCam, out dragStart);
                        fs.Clear();
                    }
                } else if(Input.GetMouseButtonUp(0) && (funcBeingTested == AdvFunction.NONE)) { // LMB released
                    if(dragging) {
                        if(!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
                            selected.Clear();
                        }

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
                            if(sm.On) {
                                foreach(LightHead alpha in msbc.Selected) {
                                    if(!selected.Contains(alpha)) {
                                        selected.Add(alpha);
                                    }
                                }
                            }
                        }

                        sbc.Selected.Clear();
                        msbc.Selected.Clear();
                        sbc.gameObject.SetActive(false);
                        msbc.gameObject.SetActive(false);
                        dragging = false;

                        BoxCollider bc = sbc.GetComponent<BoxCollider>();
                        bc.size = Vector3.zero;
                        bc.center = Vector3.zero;
                        sbc.transform.position = Vector3.zero;
                        bc = msbc.GetComponent<BoxCollider>();
                        bc.size = Vector3.zero;
                        bc.center = Vector3.zero;
                        msbc.transform.position = Vector3.zero;

                        if(selected.Count > 0) {
                            fs.Refresh();
                            fsm.Refresh();
                        }
                    }
                } else if(dragging) { // LMB held
                    bool hasHit = RectTransformUtility.ScreenPointToLocalPointInRectangle(((RectTransform)SelBox.parent), Input.mousePosition, this.myCam, out currMouse);
                    if(hasHit) {
                        sbc.gameObject.SetActive(true);
                        msbc.gameObject.SetActive(sm.On);

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

                        if(sm) {
                            positioning.x = positioning.x * -2f;
                            positioning.y = 0f;
                            positioning.z = 0f;

                            msbc.transform.localPosition = positioning;
                            ((RectTransform)msbc.transform).sizeDelta = size;
                        }
                    } else {
                        sbc.Selected.Clear();
                        SelBox.gameObject.SetActive(false);
                    }
                }

                if((myCam.pixelRect.Contains(mousePos)) && Mathf.Abs(Input.GetAxisRaw("Mouse ScrollWheel")) > 0) {
                    myCam.orthographicSize = partialOrtho = Mathf.Clamp(partialOrtho + Input.GetAxisRaw("Mouse ScrollWheel") * 1f, 1f, 10f);
                }
            }
        }
    }
}
