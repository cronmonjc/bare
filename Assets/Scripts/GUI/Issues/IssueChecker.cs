using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// GUI Item, Abstract.  Used to manage various issues that might arise.
/// </summary>
[RequireComponent(typeof(LayoutElement))]
[RequireComponent(typeof(Text))]
public abstract class IssueChecker : MonoBehaviour {
    [System.NonSerialized]
    public Text text;
    public abstract string pdfText { get; }
    protected LayoutElement le;
    
    void Start() {
        text = GetComponent<Text>();
        le = GetComponent<LayoutElement>();
    }
    
    void Update() {
        bool enable = DoCheck();
        text.enabled = enable;
        le.ignoreLayout = !enable;
    }
    
    /// <summary>
    /// Examined to see whether or not the issue being examined arises.
    /// </summary>
    /// <returns>True if there is an issue, false if there is no issue.</returns>
    public abstract bool DoCheck();
}
