using UnityEngine;
using System.Collections;

public class ProgramDirLink : DirectoryLink {
    public override void Navigate(FileBrowser fb) {
        fb.Navigate(BarManager.DirRoot);
    }
}
