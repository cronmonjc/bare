using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component, File Browser Link.  Will Navigate to the program's directory.
/// </summary>
public class ProgramDirLink : DirectoryLink {
    /// <summary>
    /// Allows the Link to tell the provided File Browser to Navigate to wherever the Link is set to go.
    /// </summary>
    /// <param name="fb">The File Browser to Navigate.</param>
    public override void Navigate(FileBrowser fb) {
        fb.Navigate(BarManager.DirRoot);
    }
}
