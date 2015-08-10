using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// UI Component, File Browser.  Generic Link to a specific directory.
/// </summary>
public class PathLink : DirectoryLink {
    /// <summary>
    /// The path this Link links to.
    /// </summary>
    public string Path;

    /// <summary>
    /// Allows the Link to tell the provided File Browser to Navigate to wherever the Link is set to go.
    /// </summary>
    /// <param name="fb">The File Browser to Navigate.</param>
    public override void Navigate(FileBrowser fb) {
        fb.Navigate(string.Join("\\", Path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)));
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        string[] pathParts = Path.Split('/', '\\');
        GetComponentInChildren<UnityEngine.UI.Text>().text = pathParts[pathParts.Length - (pathParts[pathParts.Length - 1].Length > 0 ? 1 : 2)];
    }
}
