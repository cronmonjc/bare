using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class DirectoryTree : MonoBehaviour {
    public RectTransform driveRoot;
    public GameObject ElementPrefab;

    public InputField DirInput;

    public void Clear() {
        List<Transform> temp = new List<Transform>();
        foreach(Transform alpha in driveRoot) {
            temp.Add(alpha);
        }
        foreach(Transform alpha in temp) {
            Destroy(alpha.gameObject);
        }
    }

    public void Refresh() {
        Clear();

        FileBrowser fb = FindObjectOfType<FileBrowser>();
        if(fb.drives == null || fb.drives.Length == 0)
            fb.drives = System.IO.Directory.GetLogicalDrives();
        string[] drives = fb.drives;
        string currDir = fb.currDir;

        foreach(string d in drives) {
            GameObject newbie = Instantiate<GameObject>(ElementPrefab);
            newbie.transform.SetParent(driveRoot, false);
            newbie.transform.SetAsLastSibling();
            PathLink newpl = newbie.GetComponent<PathLink>();
            newpl.Path = d;

            if(currDir.StartsWith(d.TrimEnd('/', '\\', ':'))) {
                GameObject pathList = new GameObject("currPathList");
                pathList.transform.SetParent(driveRoot, false);
                pathList.transform.SetAsLastSibling();
                VerticalLayoutGroup vlg = pathList.AddComponent<VerticalLayoutGroup>();
                vlg.childForceExpandHeight = false;
                vlg.childForceExpandWidth = true;
                vlg.childAlignment = TextAnchor.UpperCenter;
                vlg.padding = new RectOffset(8, 0, 0, 0);
                vlg.spacing = 2.0f;

                string[] pathParts = currDir.Split(new char[] {'/', '\\'}, StringSplitOptions.RemoveEmptyEntries);
                if(pathParts.Length > 1 && pathParts[1].Length > 0) {
                    for(int i = 1; i < pathParts.Length; i++) {
                        newbie = Instantiate<GameObject>(ElementPrefab);
                        newbie.transform.SetParent(pathList.transform, false);
                        newbie.transform.SetAsLastSibling();
                        newpl = newbie.GetComponent<PathLink>();
                        newpl.Path = string.Join("\\", pathParts, 0, i + 1);
                    }
                }
            }
        }

        DirInput.text = string.Join("\\", currDir.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)) + "\\";

        LayoutRebuilder.MarkLayoutForRebuild(driveRoot);
    }
}
