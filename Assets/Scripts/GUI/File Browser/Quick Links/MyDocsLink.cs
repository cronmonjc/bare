using System;
using System.Collections;
using UnityEngine;

public class MyDocsLink : DirectoryLink {
    public override void Navigate(FileBrowser fb) {
        fb.Navigate(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
    }
}
