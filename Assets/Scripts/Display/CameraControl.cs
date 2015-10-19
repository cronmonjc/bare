using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// A Component attached to the same GameObject as the Camera rendering the bar.
/// </summary>
public class CameraControl : MonoBehaviour {
    /// <summary>
    /// Are we showing the pricing in the application?
    /// </summary>
    public static bool ShowPricing = false;
    /// <summary>
    /// Is the camera trying to make the bar as large as possible?
    /// </summary>
    public static bool ShowWhole = false;

    /// <summary>
    /// If the user's box-selecting, where did they start from?
    /// </summary>
    [System.NonSerialized]
    public Vector2 dragStart;
    /// <summary>
    /// If the user's box-selecting, where are they now?
    /// </summary>
    [System.NonSerialized]
    public Vector2 currMouse;
    /// <summary>
    /// Is the user box-selecting now?
    /// </summary>
    private bool dragging;

    /// <summary>
    /// The orthographic size in use when ShowWhole is false
    /// </summary>
    [System.NonSerialized]
    public float partialOrtho = 2.0f;

    /// <summary>
    /// A reference to the FunctionSelect object.  Set via the Unity Inspector.
    /// </summary>
    public FunctionSelect fs;

    /// <summary>
    /// A reference to the staggered output toggle.  Set via the Unity Inspector.
    /// </summary>
    public RectTransform altnumber;

    /// <summary>
    /// A reference to the selection box.  Set via the Unity Inspector.
    /// </summary>
    public RectTransform SelBox;
    /// <summary>
    /// A reference to the normal Selection Box Collider
    /// </summary>
    private SelBoxCollider sbc;
    /// <summary>
    /// A reference to the mirror Selection Box Collider
    /// </summary>
    private SelBoxCollider msbc;
    /// <summary>
    /// A reference to the symmetry mode toggle
    /// </summary>
    private SymmMode sm;

    /// <summary>
    /// A reference to the Light Interaction Panel.  Set via the Unity Inspector.
    /// </summary>
    public LightInteractionPanel lip;

    /// <summary>
    /// A reference to the Camera Component on this GameObject
    /// </summary>
    private Camera myCam;

    /// <summary>
    /// A reference to the light label prefabs.  Set via the Unity Inspector.
    /// </summary>
    public GameObject LabelPrefab;
    /// <summary>
    /// Where should light heads be dropping their labels?.  Set via the Unity Inspector.
    /// </summary>
    public Transform LabelParent;

    /// <summary>
    /// A reference to the File Browser's GameObject.  Set via the Unity Inspector.
    /// </summary>
    public GameObject FBrowser;

    /// <summary>
    /// A list of all of the selected light heads
    /// </summary>
    private List<LightHead> selectedHead;
    /// <summary>
    /// A list of all of the selected bar segments / lenses
    /// </summary>
    private List<BarSegment> selectedLens;

    /// <summary>
    /// The "leave the preview" button GameObject.  Set via the Unity Inspector.
    /// </summary>
    public GameObject backButton;
    /// <summary>
    /// The preview time slider GameObject.  Set via the Unity Inspector.
    /// </summary>
    public GameObject timeSlider;
    /// <summary>
    /// The preview disclaimer box GameObject.  Set via the Unity Inspector.
    /// </summary>
    public GameObject previewDisclaimer;
    /// <summary>
    /// The function display box GameObject.  Set via the Unity Inspector.
    /// </summary>
    public GameObject funcDisplay;

    /// <summary>
    /// A reference to the function display text.  Set via the Unity Inspector.
    /// </summary>
    public Text funcDispText;

    /// <summary>
    /// A list of all of the different functions that need to be called a selection finishes.  Set via the Unity Inspector.
    /// </summary>
    public RefreshCallback RefreshOnSelect;

    /// <summary>
    /// The list of all of the selected heads.
    /// </summary>
    public List<LightHead> SelectedHead {
        get {
            return this.selectedHead;
        }
    }

    /// <summary>
    /// The list of all of the selected lenses.
    /// </summary>
    public List<BarSegment> SelectedLens {
        get {
            return this.selectedLens;
        }
    }

    /// <summary>
    /// Awake is called once, immediately as the object is created (typically at load time)
    /// </summary>
    void Awake() {
        selectedHead = new List<LightHead>();
        selectedLens = new List<BarSegment>();
    }

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        Directory.CreateDirectory("HasOpen"); // If it doesn't exist, make a new folder to store "user has app open" text files.  Because Star has issues with deleting files in use.
        StreamWriter fw = File.CreateText(Directory.GetCurrentDirectory() + "\\HasOpen\\" + System.Environment.MachineName + ".txt");
        fw.WriteLine(System.Environment.UserName + " from " + System.Environment.UserDomainName); // Store who and where in files
        fw.Flush();
        fw.Close();

        Application.targetFrameRate = 120; // Most monitors have a 59 Hz refresh rate - this just makes sure that the application will update in time

        sbc = SelBox.GetComponent<SelBoxCollider>(); // Get the Selection Box Collider Component references
        msbc = SelBox.transform.GetChild(0).GetComponent<SelBoxCollider>();

        sm = FindObjectOfType<SymmMode>(); // Find and save the reference to the Symmetry Mode toggle

        myCam = GetComponent<Camera>();
        myCam.pixelRect = new Rect(0, Screen.height * 0.6f, Screen.width, Screen.height * 0.4f - 52f); // Make sure the Camera's rendering to the right location on the screen
        float aspRatio = (myCam.pixelWidth * 1.0f) / (myCam.pixelHeight * 1.0f);
        myCam.orthographicSize = partialOrtho = (aspRatio > 5.17f ? 1.44f : (7.70737f * Mathf.Pow(aspRatio, -1.02095f))); // Adjust the orthographic size to fit within the area it's given

        RefreshOnSelect.Invoke();
    }

    /// <summary>
    /// Resets the Camera's view.  Called via UI Events.
    /// </summary>
    public void ResetView() {
        if(lip.state == LightInteractionPanel.ShowState.FUNCASSIGN) return; // If we're trying to reset view where it doesn't quite make sense, cancel
        transform.position = new Vector3(0, 0, -10); // Reset panning
        float aspRatio = (myCam.pixelWidth * 1.0f) / (myCam.pixelHeight * 1.0f);
        partialOrtho = (aspRatio > 5.17f ? 1.44f : (7.70737f * Mathf.Pow(aspRatio, -1.02095f))); // Reset zoom
    }

    /// <summary>
    /// Forces an invoke of the RefreshOnSelect callbacks.  Called via UI Events.
    /// </summary>
    public void InvokeRefresh() {
        RefreshOnSelect.Invoke();
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        Screen.SetResolution(Screen.width, Screen.height, false);  // Prevent the Unity derp with resolution
        if(!FBrowser.activeInHierarchy) { // Only do things if the File Browser isn't open
            if(Input.GetKeyDown(KeyCode.LeftBracket) && Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.RightShift)) {
                LightLabel.showBit = !LightLabel.showBit; // RCtrl + RShift + [ = show debug bits
                foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) { // Force refresh when changing debug bit display
                    alpha.Refresh();
                }
            }

            if(BarManager.inst.funcBeingTested != AdvFunction.NONE) { // If a function's being previewed...
                backButton.SetActive(true); // Show all of the preview stuff
                timeSlider.SetActive(true);
                previewDisclaimer.SetActive(true);
                funcDisplay.SetActive(true);
                switch(BarManager.inst.funcBeingTested) { // Display function name
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
            } else { // Not previewing a function
                backButton.SetActive(false); // Hide all of the preview stuff
                timeSlider.SetActive(false);
                previewDisclaimer.SetActive(false);
                funcDisplay.SetActive(false);
            }


            if(ShowWhole || lip.state == LightInteractionPanel.ShowState.FUNCASSIGN) { // Either trying to maximize view or the view's hidden anyway...
                selectedHead.Clear();
                transform.position = new Vector3(0, 0, -10); // Reset panning
                myCam.pixelRect = new Rect(0, 0, Screen.width, Screen.height); // Cam takes whole screen
                float aspRatio = (Screen.width * 1.0f) / (Screen.height * 1.0f);
                myCam.orthographicSize = (aspRatio > 5.17f ? 1.44f : (7.70737f * Mathf.Pow(aspRatio, -1.02095f))); // Adjust ortho size
            } else {
                myCam.pixelRect = new Rect(0, Screen.height * 0.6f, Screen.width, Screen.height * 0.4f - 52f); // Cam sits in its portion of the screen

                Vector2 mousePos = Input.mousePosition; // Get mouse position
                if(Input.GetMouseButtonDown(0) && (BarManager.inst.funcBeingTested == AdvFunction.NONE)) { // LMB pressed
                    Camera UICam = GameObject.Find("UI").GetComponent<Camera>();  // Find the UI Camera

                    bool cont = true;

                    foreach(CollapsingMenuControl cmc in FindObjectsOfType<CollapsingMenuControl>()) {
                        if(RectTransformUtility.RectangleContainsScreenPoint(cmc.transform as RectTransform, Input.mousePosition, UICam)) {
                            cont = false; // Mouse is over a menu, don't try to select
                        }
                    }

                    if(RectTransformUtility.RectangleContainsScreenPoint(altnumber, Input.mousePosition, myCam)) {
                        // If the staggered output toggle is visible and the mouse is over it, don't try to select
                        cont &= !altnumber.GetChild(0).gameObject.activeInHierarchy;
                    }

                    if(cont)  // If we can select...
                        if(myCam.pixelRect.Contains(mousePos)) { // ...and the camera rectangle contains the mouse...
                            // ...start selecting.
                            dragging = RectTransformUtility.ScreenPointToLocalPointInRectangle(((RectTransform)SelBox.parent), Input.mousePosition, this.myCam, out dragStart);
                            fs.Clear();
                            if(ErrorLogging.logInput) ErrorLogging.LogInput("Began Selection");
                        }
                } else if(Input.GetMouseButtonUp(0) && (BarManager.inst.funcBeingTested == AdvFunction.NONE)) { // LMB released
                    if(dragging) {  // Only do things if we've been selecting
                        if(ErrorLogging.logInput) ErrorLogging.LogInput("Finished Selection");

                        if(!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
                            // Clear unless either control key is held
                            selectedHead.Clear();
                            selectedLens.Clear();
                        }

                        if(sbc.SelectedHead.Count == 0) { // There isn't any heads in the selection box.
                            if(sbc.SelectedLens.Count == 0) { // No Lenses either.
                                RaycastHit hit;  // Test if there's a light under the cursor?
                                if(Physics.Raycast(myCam.ScreenPointToRay(Input.mousePosition), out hit)) { // There WAS something!
                                    LightHead head = hit.transform.GetComponent<LightHead>();
                                    if(head != null) {
                                        selectedHead.Add(head);

                                        if(ErrorLogging.logInput) ErrorLogging.LogInput("Single-selected head: " + head.transform.GetPath());
                                    }
                                }
                            } else { // Lenses selected
                                selectedHead.Clear(); // Don't care 'bout heads no mo'
                                int count = selectedLens.Count;
                                foreach(BarSegment alpha in sbc.SelectedLens) {
                                    if(!selectedLens.Contains(alpha)) { // Add selected lenses unless they are already in it
                                        selectedLens.Add(alpha);
                                    }
                                }
                                if(sm.On) { // Only if symmetry mode is on...
                                    foreach(BarSegment alpha in msbc.SelectedLens) {
                                        if(!selectedLens.Contains(alpha)) { // Add symmetrically selected lenses unless they are already in it
                                            selectedLens.Add(alpha);
                                        }
                                    }
                                }
                                if(selectedLens.Count == count) { // If no new lenses were added, user must be trying to remove from list
                                    foreach(BarSegment alpha in sbc.SelectedLens) {
                                        if(selectedLens.Contains(alpha)) { // If selected lens is in list, remove
                                            selectedLens.Remove(alpha);
                                        }
                                    }
                                    if(sm.On) { // Only if symmetry mode is on...
                                        foreach(BarSegment alpha in msbc.SelectedLens) {
                                            if(selectedLens.Contains(alpha)) { // If symmetrically selected lens is in list, remove
                                                selectedLens.Remove(alpha);
                                            }
                                        }
                                    }
                                }
                                if(ErrorLogging.logInput) { // Log lens selection if desired
                                    System.Text.StringBuilder inputBuilder = new System.Text.StringBuilder();
                                    for(byte i = 0; i < selectedLens.Count; i++) {
                                        inputBuilder.Append("\n    ");
                                        inputBuilder.Append(selectedLens[i].transform.GetPath());
                                    }

                                    ErrorLogging.LogInput("Box-selected lenses: " + inputBuilder.ToString());
                                }
                            }
                        } else { // Heads selected
                            selectedLens.Clear(); // Don't care 'bout lenses no mo'
                            int count = selectedHead.Count;
                            foreach(LightHead alpha in sbc.SelectedHead) {
                                if(!selectedHead.Contains(alpha)) { // Add selected heads unless they are already in it
                                    selectedHead.Add(alpha);
                                }
                            }
                            if(sm.On) { // Only if symmetry mode is on...
                                foreach(LightHead alpha in msbc.SelectedHead) {
                                    if(!selectedHead.Contains(alpha)) { // Add symmetrically selected heads unless they are already in it
                                        selectedHead.Add(alpha);
                                    }
                                }
                            }
                            if(selectedHead.Count == count) { // If no new heads were added, user must be trying to remove from list
                                foreach(LightHead alpha in sbc.SelectedHead) {
                                    if(selectedHead.Contains(alpha)) { // If selected head is in list, remove
                                        selectedHead.Remove(alpha);
                                    }
                                }
                                if(sm.On) { // Only if symmetry mode is on...
                                    foreach(LightHead alpha in msbc.SelectedHead) {
                                        if(selectedHead.Contains(alpha)) { // If symmetrically selected head is in list, remove
                                            selectedHead.Remove(alpha);
                                        }
                                    }
                                }
                            }
                            
                            if(ErrorLogging.logInput) { // Log head selection if desired
                                System.Text.StringBuilder inputBuilder = new System.Text.StringBuilder();
                                for(byte i = 0; i < selectedHead.Count; i++) {
                                    inputBuilder.Append("\n    ");
                                    inputBuilder.Append(selectedHead[i].transform.GetPath());
                                }

                                ErrorLogging.LogInput("Box-selected heads: " + inputBuilder.ToString()); 
                            }
                        }

                        sbc.SelectedHead.Clear(); // Empty out the Selection Box Colliders
                        sbc.SelectedLens.Clear();
                        msbc.SelectedHead.Clear();
                        msbc.SelectedLens.Clear();
                        sbc.gameObject.SetActive(false);
                        msbc.gameObject.SetActive(false);
                        dragging = false;

                        BoxCollider bc = sbc.GetComponent<BoxCollider>(); // Hide away the Selection Box Colliders
                        bc.size = Vector3.zero;
                        bc.center = Vector3.zero;
                        sbc.transform.position = Vector3.zero;
                        bc = msbc.GetComponent<BoxCollider>();
                        bc.size = Vector3.zero;
                        bc.center = Vector3.zero;
                        msbc.transform.position = Vector3.zero;

                        if(FunctionEditPane.currFunc != AdvFunction.NONE) { // If we're editing a function, retest the function editing stuff
                            FunctionEditPane.RetestStatic();
                        } else {
                            RefreshOnSelect.Invoke(); // Call everything on the one callback list
                        }
                    }
                } else if(dragging) { // LMB held
                    bool hasHit = RectTransformUtility.ScreenPointToLocalPointInRectangle(((RectTransform)SelBox.parent), Input.mousePosition, this.myCam, out currMouse); // Figure out where the mouse is in-world
                    if(hasHit) {
                        sbc.gameObject.SetActive(true); // Always show main SBC
                        msbc.gameObject.SetActive(sm.On); // Only show mirror SBC if symmetry select mode is on

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

                        SelBox.localPosition = positioning; // Move and resize the box so that it can both give the appearance of selecting as well as collide with the heads and lenses
                        SelBox.sizeDelta = size;

                        if(sm.On) { // If symmetry select mode is on...
                            positioning.x = positioning.x * -2f;
                            positioning.y = 0f;
                            positioning.z = 0f;

                            msbc.transform.localPosition = positioning; // Move and resize the mirror box too
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

                if(!FollowMouse.BlockMouseInput && r.Contains(mousePos)) { // If we're not blocking mouse input and the mouse is over the bar camera rectangle...
                    partialOrtho = Mathf.Clamp(partialOrtho + Input.GetAxisRaw("Mouse ScrollWheel") * 1f, 1f, 10f); // Mousewheel zoom
                    if(Input.GetMouseButton(1)) // RMB Pan
                        transform.position -= (new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"), 0f) * myCam.orthographicSize * 0.1f);
                }
                myCam.orthographicSize = partialOrtho; // Apply zoom level
            }
        }
    }

    /// <summary>
    /// Called when the application is about to quit
    /// </summary>
    void OnApplicationQuit() {
        if(Directory.Exists(Directory.GetCurrentDirectory() + "\\tempgen")) // Generated some files for the PDF
            Directory.Delete(Directory.GetCurrentDirectory() + "\\tempgen", true); // Delete folder to clean up after self
        File.Delete(Directory.GetCurrentDirectory() + "\\HasOpen\\" + System.Environment.MachineName + ".txt"); // User no longer has application open, remove file
    }
}

/// <summary>
/// Unity's equivalent of a delegate for the refresh callback.  Allows callbacks to be assigned in the Inspector.
/// </summary>
[System.Serializable]
public class RefreshCallback : UnityEngine.Events.UnityEvent {

}