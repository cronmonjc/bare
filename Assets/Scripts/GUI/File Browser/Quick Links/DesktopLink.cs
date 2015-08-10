using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// UI Component, File Browser Link.  Will Navigate to the Desktop.
/// </summary>
public class DesktopLink : DirectoryLink {
    /// <summary>
    /// Allows the Link to tell the provided File Browser to Navigate to wherever the Link is set to go.
    /// </summary>
    /// <param name="fb">The File Browser to Navigate.</param>
    public override void Navigate(FileBrowser fb) {
        fb.Navigate(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
    }
}
