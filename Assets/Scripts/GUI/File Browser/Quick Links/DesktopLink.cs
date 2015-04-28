using System;
using System.Collections;
using UnityEngine;

public class DesktopLink : DirectoryLink {
    public override void Navigate(FileBrowser fb) {
        fb.Navigate(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
    }
}
