using System;
using System.Collections;
using UnityEngine;

public class PathLink : DirectoryLink {
    public string Path;

    public override void Navigate(FileBrowser fb) {
        fb.Navigate(string.Join("/", Path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)) + "/");
    }

    void Update() {
        string[] pathParts = Path.Split('/', '\\');
        GetComponentInChildren<UnityEngine.UI.Text>().text = pathParts[pathParts.Length - (pathParts[pathParts.Length - 1].Length > 0 ? 1 : 2)];
    }
}
