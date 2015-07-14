using UnityEngine;
using System.Collections;

public class UpAFolder : MonoBehaviour {
    public void Clicked() {
        FileBrowser fb = FindObjectOfType<FileBrowser>();

        string[] parts = fb.currDir.Split(new char[] {'/', '\\'}, System.StringSplitOptions.RemoveEmptyEntries);

        if(parts.Length > 1)
            fb.Navigate(string.Join("\\", parts, 0, parts.Length - 1));
    }
}
