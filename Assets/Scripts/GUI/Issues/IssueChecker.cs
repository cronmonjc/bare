using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UI Component, Issue, Abstract.  Used to manage various issues that might arise.
/// </summary>
[RequireComponent(typeof(LayoutElement))]
[RequireComponent(typeof(Text))]
public abstract class IssueChecker : MonoBehaviour {
    /// <summary>
    /// The Text Component that this issue uses.
    /// </summary>
    [System.NonSerialized]
    public Text text;
    /// <summary>
    /// Gets the text used to describe an issue on the exported PDF.
    /// </summary>
    public abstract string pdfText { get; }
    /// <summary>
    /// The Layout Element object
    /// </summary>
    protected LayoutElement le;

    /// <summary>
    /// Value indicating whether or not this issue ignores suppression from viewing an unmodified bar.  Set via Unity Inspector.
    /// </summary>
    public bool IgnoreSuppression;


    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        text = GetComponent<Text>();
        le = GetComponent<LayoutElement>();
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        bool enable = (IgnoreSuppression || BarManager.moddedBar) && DoCheck();
        text.enabled = enable;
        le.ignoreLayout = !enable;
    }
    
    /// <summary>
    /// Examined to see whether or not the issue being examined arises.
    /// </summary>
    /// <returns>True if there is an issue, false if there is no issue.</returns>
    public abstract bool DoCheck();
}
