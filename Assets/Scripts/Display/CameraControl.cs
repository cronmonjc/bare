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

    public RectTransform altnumber;

    public RectTransform SelBox;
    private SelBoxCollider sbc, msbc;
    private SymmMode sm;

    public LightInteractionPanel lip;

    private Camera myCam;

    public GameObject LabelPrefab;
    public Transform LabelParent;

    public GameObject FuncSelectRoot;

    public GameObject FBrowser;

    private List<LightHead> selectedHead;
    private List<BarSegment> selectedLens;

    public GameObject backButton;
    public GameObject timeSlider;
    public GameObject previewDisclaimer;
    public GameObject funcDisplay;
    public Text funcDispText;

    public RefreshCallback RefreshOnSelect;

    public List<LightHead> SelectedHead {
        get {
            if(dragging) {
                return sbc.SelectedHead;
            } else {
                return this.selectedHead;
            }
        }
    }

    public List<LightHead> OnlyCamSelectedHead {
        get {
            return this.selectedHead;
        }
    }

    public List<BarSegment> SelectedLens {
        get {
            return this.selectedLens;
        }
    }

    void Awake() {
        selectedHead = new List<LightHead>();
        selectedLens = new List<BarSegment>();
    }

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
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
        myCam.pixelRect = new Rect(0, Screen.height * 0.6f, Screen.width, Screen.height * 0.4f - 52f);
        float aspRatio = (myCam.pixelWidth * 1.0f) / (myCam.pixelHeight * 1.0f);
        myCam.orthographicSize = partialOrtho = (aspRatio > 3.97f ? 1.985f : (7.88f * Mathf.Pow(aspRatio, -0.99787f)));

        RefreshOnSelect.Invoke();
    }

    public void ResetView() {
        if(lip.state == LightInteractionPanel.ShowState.FUNCASSIGN) return;
        transform.position = new Vector3(0, 0, -10);
        float aspRatio = (myCam.pixelWidth * 1.0f) / (myCam.pixelHeight * 1.0f);
        partialOrtho = (aspRatio > 3.97f ? 1.985f : (7.88f * Mathf.Pow(aspRatio, -0.99787f)));
    }

    public void InvokeRefresh() {
        RefreshOnSelect.Invoke();
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        Screen.SetResolution(Screen.width, Screen.height, false);
        if(!FBrowser.activeInHierarchy) {
            if(Input.GetKeyDown(KeyCode.LeftBracket) && Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.RightShift)) {
                LightLabel.showBit = !LightLabel.showBit;
                foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) {
                    alpha.Refresh();
                }
            }

            if(BarManager.inst.funcBeingTested != AdvFunction.NONE) {
                backButton.SetActive(true);
                timeSlider.SetActive(true);
                previewDisclaimer.SetActive(true);
                funcDisplay.SetActive(true);
                switch(BarManager.inst.funcBeingTested) {
                    case AdvFunction.PRIO1:
                        funcDispText.text = "Previewing Function: Priority 1";
                        break;
                    case AdvFunction.PRIO2:
                        funcDispText.text = "Previewing Function: Priority 2";
                        break;
                    case AdvFunction.PRIO3:
                        funcDispText.text = "Previewing Function: Priority 3";
                        break;
                    case AdvFunction.PRIO4:
                        funcDispText.text = "Previewing Function: Priority 4";
                        break;
                    case AdvFunction.PRIO5:
                        funcDispText.text = "Previewing Function: Priority 5";
                        break;
                    case AdvFunction.FTAKEDOWN:
                        funcDispText.text = "Previewing Function: Flashing Pursuit";
                        break;
                    case AdvFunction.FALLEY:
                        funcDispText.text = "Previewing Function: Flashing Alley";
                        break;
                    case AdvFunction.ICL:
                        funcDispText.text = "Previewing Function: ICL";
                        break;
                    case AdvFunction.TRAFFIC_LEFT:
                        funcDispText.text = "Previewing Function: Direct Left";
                        break;
                    case AdvFunction.TRAFFIC_RIGHT:
                        funcDispText.text = "Previewing Function: Direct Right";
                        break;
                    default:
                        funcDispText.text = "Previewing Unknown Function";
                        break;
                }
            } else {
                backButton.SetActive(false);
                timeSlider.SetActive(false);
                previewDisclaimer.SetActive(false);
                funcDisplay.SetActive(false);
            }


            if(ShowWhole || lip.state == LightInteractionPanel.ShowState.FUNCASSIGN) {
                selectedHead.Clear();
                myCam.pixelRect = new Rect(0, 0, Screen.width, Screen.height);
                float aspRatio = (Screen.width * 1.0f) / (Screen.height * 1.0f);
                myCam.orthographicSize = (aspRatio > 3.97f ? 1.985f : (7.88f * Mathf.Pow(aspRatio, -0.99787f)));
            } else {
                myCam.pixelRect = new Rect(0, Screen.height * 0.6f, Screen.width, Screen.height * 0.4f - 52f);

                Vector2 mousePos = Input.mousePosition;
                if(Input.GetMouseButtonDown(0) && (BarManager.inst.funcBeingTested == AdvFunction.NONE)) { // LMB pressed
                    Camera UICam = GameObject.Find("UI").GetComponent<Camera>();

                    bool cont = true;

                    foreach(CollapsingMenuControl cmc in FindObjectsOfType<CollapsingMenuControl>()) {
                        if(RectTransformUtility.RectangleContainsScreenPoint(cmc.transform as RectTransform, Input.mousePosition, UICam)) {
                            cont = false;
                        }
                    }

                    if(RectTransformUtility.RectangleContainsScreenPoint(altnumber, Input.mousePosition, myCam)) {
                        cont &= !altnumber.GetChild(0).gameObject.activeInHierarchy;
                    }

                    if(cont)
                        if(myCam.pixelRect.Contains(mousePos)) {
                            dragging = RectTransformUtility.ScreenPointToLocalPointInRectangle(((RectTransform)SelBox.parent), Input.mousePosition, this.myCam, out dragStart);
                            fs.Clear();
                            if(ErrorLogging.logInput) ErrorLogging.LogInput("Began Selection");
                        }
                } else if(Input.GetMouseButtonUp(0) && (BarManager.inst.funcBeingTested == AdvFunction.NONE)) { // LMB released
                    if(dragging) {
                        if(ErrorLogging.logInput) ErrorLogging.LogInput("Finished Selection");

                        if(!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
                            selectedHead.Clear();
                            selectedLens.Clear();
                        }

                        if(SelectedHead.Count == 0) { // There isn't any heads in the selection box.
                            if(sbc.SelectedLens.Count == 0) { // No Lenses either.
                                RaycastHit hit;  // Test if there's a light under the cursor?
                                if(Physics.Raycast(myCam.ScreenPointToRay(Input.mousePosition), out hit)) { // There WAS something!
                                    LightHead head = hit.transform.GetComponent<LightHead>();
                                    if(head != null) {
                                        selectedHead.Add(head);

                                        if(ErrorLogging.logInput) ErrorLogging.LogInput("Single-selected head: " + head.transform.GetPath());
                                    }
                                }
                            } else {
                                selectedHead.Clear();
                                int count = selectedLens.Count;
                                foreach(BarSegment alpha in sbc.SelectedLens) {
                                    if(!selectedLens.Contains(alpha)) {
                                        selectedLens.Add(alpha);
                                    }
                                }
                                if(sm.On) {
                                    foreach(BarSegment alpha in msbc.SelectedLens) {
                                        if(!selectedLens.Contains(alpha)) {
                                            selectedLens.Add(alpha);
                                        }
                                    }
                                }
                                if(selectedLens.Count == count) {
                                    foreach(BarSegment alpha in sbc.SelectedLens) {
                                        if(selectedLens.Contains(alpha)) {
                                            selectedLens.Remove(alpha);
                                        }
                                    }
                                    if(sm.On) {
                                        foreach(BarSegment alpha in msbc.SelectedLens) {
                                            if(selectedLens.Contains(alpha)) {
                                                selectedLens.Remove(alpha);
                                            }
                                        }
                                    }
                                }
                                if(ErrorLogging.logInput) {
                                    System.Text.StringBuilder inputBuilder = new System.Text.StringBuilder();
                                    for(byte i = 0; i < selectedLens.Count; i++) {
                                        inputBuilder.Append("\n    ");
                                        inputBuilder.Append(selectedLens[i].transform.GetPath());
                                    }

                                    ErrorLogging.LogInput("Box-selected lenses: " + inputBuilder.ToString());
                                }
                            }
                        } else {
                            selectedLens.Clear();
                            int count = selectedHead.Count;
                            foreach(LightHead alpha in sbc.SelectedHead) {
                                if(!selectedHead.Contains(alpha)) {
                                    selectedHead.Add(alpha);
                                }
                            }
                            if(sm.On) {
                                foreach(LightHead alpha in msbc.SelectedHead) {
                                    if(!selectedHead.Contains(alpha)) {
                                        selectedHead.Add(alpha);
                                    }
                                }
                            }
                            if(selectedHead.Count == count) {
                                foreach(LightHead alpha in sbc.SelectedHead) {
                                    if(selectedHead.Contains(alpha)) {
                                        selectedHead.Remove(alpha);
                                    }
                                }
                                if(sm.On) {
                                    foreach(LightHead alpha in msbc.SelectedHead) {
                                        if(selectedHead.Contains(alpha)) {
                                            selectedHead.Remove(alpha);
                                        }
                                    }
                                }
                            }
                            
                            if(ErrorLogging.logInput) {
                                System.Text.StringBuilder inputBuilder = new System.Text.StringBuilder();
                                for(byte i = 0; i < selectedHead.Count; i++) {
                                    inputBuilder.Append("\n    ");
                                    inputBuilder.Append(selectedHead[i].transform.GetPath());
                                }

                                ErrorLogging.LogInput("Box-selected heads: " + inputBuilder.ToString()); 
                            }
                        }

                        sbc.SelectedHead.Clear();
                        sbc.SelectedLens.Clear();
                        msbc.SelectedHead.Clear();
                        msbc.SelectedLens.Clear();
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
                        } else {
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
                        sbc.SelectedHead.Clear();
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