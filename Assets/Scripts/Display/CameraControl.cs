using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class CameraControl : MonoBehaviour {
    public static bool ShowPricing = false;
    public static bool ShowWhole = false;

    public Transform pivot;
    public Vector2 dragStart, currMouse;
    private bool dragging;

    [System.NonSerialized]
    public float partialOrtho = 2.0f;

    public FunctionSelect fs;

    public RectTransform cover;

    public RectTransform SelBox;
    private SelBoxCollider sbc, msbc;
    private SymmMode sm;

    public LightInteractionPanel lip;

    private Camera myCam;

    public GameObject LabelPrefab;
    public Transform LabelParent;

    public GameObject FuncSelectRoot;

    public GameObject FBrowser;

    private List<LightHead> selected;

    public GameObject backButton;
    public GameObject timeSlider;

    public RefreshCallback RefreshOnSelect;

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
        Directory.CreateDirectory("HasOpen");
        StreamWriter fw = File.CreateText(Directory.GetCurrentDirectory() + "\\HasOpen\\" + System.Environment.MachineName + ".txt");
        fw.WriteLine(System.Environment.UserName + " from " + System.Environment.UserDomainName);
        fw.Flush();
        fw.Close();

        Application.targetFrameRate = 120;

        sbc = SelBox.GetComponent<SelBoxCollider>();
        msbc = SelBox.transform.GetChild(0).GetComponent<SelBoxCollider>();

        sm = FindObjectOfType<SymmMode>();

        myCam = GetComponent<Camera>();
        myCam.pixelRect = new Rect(0, Screen.height * 0.6f, Screen.width, Screen.height * 0.4f - 32f);
        float aspRatio = (myCam.pixelWidth * 1.0f) / (myCam.pixelHeight * 1.0f);
        myCam.orthographicSize = partialOrtho = (aspRatio > 3.97f ? 1.985f : (7.86225f * Mathf.Pow(aspRatio, -0.99787f)));
    }

    public void ResetView() {
        transform.position = new Vector3(0, 0, -10);
        float aspRatio = (myCam.pixelWidth * 1.0f) / (myCam.pixelHeight * 1.0f);
        partialOrtho = (aspRatio > 3.97f ? 1.985f : (7.86225f * Mathf.Pow(aspRatio, -0.99787f)));

    }

    public void InvokeRefresh() {
        RefreshOnSelect.Invoke();
    }

    void Update() {
        Screen.SetResolution(Screen.width, Screen.height, false);
        if(!FBrowser.activeInHierarchy) {
            if(Input.GetKeyDown(KeyCode.LeftBracket) && Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.RightShift)) {
                LightLabel.showBit = !LightLabel.showBit;
                foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) {
                    alpha.Refresh();
                }
            }

            backButton.SetActive(BarManager.inst.funcBeingTested != AdvFunction.NONE);
            timeSlider.SetActive(BarManager.inst.funcBeingTested != AdvFunction.NONE);

            if(ShowWhole || lip.state == LightInteractionPanel.ShowState.FUNCASSIGN) {
                selected.Clear();
                myCam.pixelRect = new Rect(0, 0, Screen.width, Screen.height);
                float aspRatio = (Screen.width * 1.0f) / (Screen.height * 1.0f);
                myCam.orthographicSize = (aspRatio > 3.97f ? 1.985f : (7.86225f * Mathf.Pow(aspRatio, -0.99787f)));
            } else {
                myCam.pixelRect = new Rect(0, Screen.height * 0.6f, Screen.width, Screen.height * 0.4f - 32f);

                Vector2 mousePos = Input.mousePosition;
                if(Input.GetMouseButtonDown(0) && (BarManager.inst.funcBeingTested == AdvFunction.NONE)) { // LMB pressed
                    Camera UICam = GameObject.Find("UI").GetComponent<Camera>();

                    bool cont = true;

                    foreach(CollapsingMenuControl cmc in FindObjectsOfType<CollapsingMenuControl>()) {
                        if(RectTransformUtility.RectangleContainsScreenPoint(cmc.transform as RectTransform, Input.mousePosition, UICam)) {
                            cont = false;
                        }
                    }

                    if(cont)
                        if(myCam.pixelRect.Contains(mousePos)) {
                            dragging = RectTransformUtility.ScreenPointToLocalPointInRectangle(((RectTransform)SelBox.parent), Input.mousePosition, this.myCam, out dragStart);
                            fs.Clear();
                        }
                } else if(Input.GetMouseButtonUp(0) && (BarManager.inst.funcBeingTested == AdvFunction.NONE)) { // LMB released
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

                        if(FunctionEditPane.currFunc != AdvFunction.NONE) {
                            FunctionEditPane.RetestStatic();
                        } else if(selected.Count > 0) {
                            RefreshOnSelect.Invoke();
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

                Rect r = myCam.pixelRect;
                r.x += 5;
                r.y += 5;
                r.width -= 10;
                r.height -= 10;

                if(!FollowMouse.BlockMouseInput && r.Contains(mousePos)) {
                    partialOrtho = Mathf.Clamp(partialOrtho + Input.GetAxisRaw("Mouse ScrollWheel") * 1f, 1f, 10f);
                    if(Input.GetMouseButton(1))
                        transform.position -= (new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"), 0f) * myCam.orthographicSize * 0.1f);
                }
                myCam.orthographicSize = partialOrtho;
            }
        }
    }

    void OnApplicationQuit() {
        if(Directory.Exists(Directory.GetCurrentDirectory() + "\\tempgen"))
            Directory.Delete(Directory.GetCurrentDirectory() + "\\tempgen", true);
        File.Delete(Directory.GetCurrentDirectory() + "\\HasOpen\\" + System.Environment.MachineName + ".txt");
    }
}

[System.Serializable]
public class RefreshCallback : UnityEngine.Events.UnityEvent {

}