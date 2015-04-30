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

    private float height = 1.0f;
    public RectTransform cover;

    public RectTransform SelBox;
    private SelBoxCollider sbc;

    private Camera myCam, childCam;

    public GameObject LabelPrefab;
    public Transform LabelParent;

    public GameObject FuncSelectRoot;

    public GameObject FBrowser;

    private List<LightBlock> selected;

    public List<LightBlock> Selected {
        get {
            if(dragging) {
                return sbc.Selected;
            } else {
                return this.selected;
            }
        }
    }

    public List<LightBlock> OnlyCamSelected {
        get {
            return this.selected;
        }
    }

    void Start() {
        Application.targetFrameRate = 120;

        selected = new List<LightBlock>();
        sbc = SelBox.GetComponent<SelBoxCollider>();

        myCam = GetComponent<Camera>();
        childCam = transform.FindChild("LabelCamera").GetComponent<Camera>();
    }

    void Update() {
        if(!FBrowser.activeInHierarchy) {
            GetComponent<Light>().enabled = (funcBeingTested == Function.NONE);

            Vector2 mousePos = Input.mousePosition;
            if(Input.GetMouseButtonDown(0) && (funcBeingTested == Function.NONE)) { // LMB pressed
                if(Selected.Count == 0 || mousePos.y > 0.45f * Screen.height) {
                    dragging = RectTransformUtility.ScreenPointToLocalPointInRectangle(((RectTransform)SelBox.parent), Input.mousePosition, this.myCam, out dragStart);
                    os.Clear();
                }
            } else if(Input.GetMouseButtonUp(0) && (funcBeingTested == Function.NONE)) { // LMB released
                if(dragging && (OnlyCamSelected.Count == 0 || mousePos.y > 0.45f * Screen.height)) {
                    if(!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
                        selected.Clear();
                    }

                    if(dragging) {
                        if(Selected.Count == 0) { // There isn't anything in the selection box.  Test if there was a light under the cursor?
                            RaycastHit hit;
                            if(Physics.Raycast(myCam.ScreenPointToRay(Input.mousePosition), out hit)) { // There WAS something!
                                LightBlock head = hit.transform.GetComponent<LightBlock>();
                                if(head != null) {
                                    selected.Add(head);
                                }
                            }
                        } else {
                            foreach(LightBlock alpha in sbc.Selected) {
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

                    if(size.x < 800 && size.y < 600) {
                        SelBox.localPosition = positioning;
                        SelBox.sizeDelta = size;
                    } else {
                        sbc.Selected.Clear();
                        SelBox.gameObject.SetActive(false);
                    }
                } else {
                    sbc.Selected.Clear();
                    SelBox.gameObject.SetActive(false);
                }
            }

            if(Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            if(Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2)) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if(Input.GetMouseButton(1)) { // RMB Held
                Vector3 euler = pivot.localEulerAngles;
                euler.x = Mathf.Clamp(euler.x + (Input.GetAxisRaw("Mouse Y") * 5f), 280f, 350f);
                euler.y = euler.y + (Input.GetAxisRaw("Mouse X") * 5f);
                pivot.localEulerAngles = euler;
            }

            if(Input.GetMouseButton(2)) { // MMB Held
                Vector3 newPos = pivot.position;

                newPos += ((transform.right * -1f * Input.GetAxisRaw("Mouse X")) + (Vector3.Cross(Vector3.up, transform.right) * Input.GetAxisRaw("Mouse Y")));
                newPos.x = Mathf.Clamp(newPos.x, -20f, 20f);
                newPos.z = Mathf.Clamp(newPos.z, -20f, 20f);

                pivot.position = newPos;
            }

            if((selected.Count == 0 || mousePos.y > 0.45f * Screen.height) && Mathf.Abs(Input.GetAxisRaw("Mouse ScrollWheel")) > 0) {
                myCam.fieldOfView = Mathf.Clamp(myCam.fieldOfView + Input.GetAxisRaw("Mouse ScrollWheel") * 20f, 10, 90f);
                childCam.fieldOfView = myCam.fieldOfView;
            }
        }

        float slideTo = 1.0f;
        if(selected.Count > 0) {
            slideTo = 0.55f;
        }

        if(height != slideTo) {
            height = Mathf.Lerp(height, slideTo, Time.deltaTime * 5.0f);

            if(Mathf.Abs(slideTo - height) < 0.002f) {
                height = slideTo;
            }

            myCam.pixelRect = new Rect(0, Screen.height * (1f - height), Screen.width, Screen.height * height);
            childCam.pixelRect = new Rect(0, Screen.height * (1f - height), Screen.width, Screen.height * height);
            cover.anchorMax = new Vector2(1.0f, (1f - height));
        }
    }
}
