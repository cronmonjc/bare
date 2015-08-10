using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// UI Component, File Browser.  Constructs the directory listing on the left pane.
/// </summary>
public class DirectoryTree : MonoBehaviour {
    /// <summary>
    /// The drive root where drives get rendered
    /// </summary>
    public RectTransform driveRoot;
    /// <summary>
    /// The element prefab used to generate each listing
    /// </summary>
    public GameObject ElementPrefab;

    /// <summary>
    /// The directory input field
    /// </summary>
    public InputField DirInput;

    /// <summary>
    /// Clears the existing list.
    /// </summary>
    public void Clear() {
        List<Transform> temp = new List<Transform>();
        foreach(Transform alpha in driveRoot) {
            temp.Add(alpha);
        }
        foreach(Transform alpha in temp) {
            Destroy(alpha.gameObject);
        }
    }

    /// <summary>
    /// Refreshes the list.  Called when Navigating.
    /// </summary>
    public void Refresh() {
        Clear();

        FileBrowser fb = FindObjectOfType<FileBrowser>();
        if(fb.drives == null || fb.drives.Length == 0) // Don't have drives yet, fetch
            fb.drives = System.IO.Directory.GetLogicalDrives();
        string[] drives = fb.drives; // Get from cache
        string currDir = fb.currDir; // Get current directory

        foreach(string d in drives) {
            #region Create Root Drive link
            GameObject newbie = Instantiate<GameObject>(ElementPrefab);
            newbie.transform.SetParent(driveRoot, false);
            newbie.transform.SetAsLastSibling();
            PathLink newpl = newbie.GetComponent<PathLink>();
            newpl.Path = d; 
            #endregion

            if(currDir.StartsWith(d.TrimEnd('/', '\\', ':'))) {  // If our current directory starts at the specified drive...
                #region Generate a new GameObject to contain links for the current path
                GameObject pathList = new GameObject("currPathList");
                pathList.transform.SetParent(driveRoot, false);
                pathList.transform.SetAsLastSibling();
                VerticalLayoutGroup vlg = pathList.AddComponent<VerticalLayoutGroup>();
                vlg.childForceExpandHeight = false;
                vlg.childForceExpandWidth = true;
                vlg.childAlignment = TextAnchor.UpperCenter;
                vlg.padding = new RectOffset(8, 0, 0, 0);
                vlg.spacing = 2.0f; 
                #endregion

                #region Create the links for the current path
                string[] pathParts = currDir.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                if(pathParts.Length > 1 && pathParts[1].Length > 0) {
                    for(int i = 1; i < pathParts.Length; i++) {
                        newbie = Instantiate<GameObject>(ElementPrefab);
                        newbie.transform.SetParent(pathList.transform, false);
                        newbie.transform.SetAsLastSibling();
                        newpl = newbie.GetComponent<PathLink>();
                        newpl.Path = string.Join("\\", pathParts, 0, i + 1);
                    }
                } 
                #endregion
            }
        }

        // Set the text on the drectory input field to the current path
        DirInput.text = string.Join("\\", currDir.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)) + "\\";

        // Have Unity fix the layout for the drive / directory listing
        LayoutRebuilder.MarkLayoutForRebuild(driveRoot);
    }
}
