using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// UI Component, File Browser.  Contains and manages all of the file items.
/// </summary>
public class FileListing : MonoBehaviour {
    /// <summary>
    /// The default file icon, used when the type is not recognized.  Set via Unity Inspector.
    /// </summary>
    public Sprite defaultFile;
    /// <summary>
    /// The diretory icon, used when the file is a directory.  Set via Unity Inspector.
    /// </summary>
    public Sprite dirIco;
    /// <summary>
    /// A struct containing information about a specially-recognized file type.
    /// </summary>
    [System.Serializable]
    public struct Icon {
        /// <summary>
        /// The extension used.  (Called name to be easily findable in the Inspector.)  Set via Unity Inspector.
        /// </summary>
        public string name;
        /// <summary>
        /// The image used.  Set via Unity Inspector.
        /// </summary>
        public Sprite image;
    }
    /// <summary>
    /// The list of special icons to use.  Set via Unity Inspector.
    /// </summary>
    public Icon[] specialIcons;

    /// <summary>
    /// The root of where every file item will go.  Set via Unity Inspector.
    /// </summary>
    public RectTransform fileRoot;
    /// <summary>
    /// The file item element prefab.  Set via Unity Inspector.
    /// </summary>
    public GameObject ElementPrefab;

    /// <summary>
    /// Clears the list.
    /// </summary>
    public void Clear() {
        List<Transform> temp = new List<Transform>();
        foreach(Transform alpha in fileRoot) {
            temp.Add(alpha);
        }
        foreach(Transform alpha in temp) {
            Destroy(alpha.gameObject);
        }

        FileItem.SelectedFile = null;
    }

    /// <summary>
    /// Refreshes the list.
    /// </summary>
    public void Refresh() {
        Clear();

        FileBrowser fb = FindObjectOfType<FileBrowser>();
        string currDir = fb.currDir;

        string[] directories = Directory.GetDirectories(currDir); // Get all the directories and files that reside at this spot
        string[] files = Directory.GetFiles(currDir);

        // Do all directories first
        foreach(string dir in directories) {
            GameObject newbie = Instantiate<GameObject>(ElementPrefab);
            newbie.transform.SetParent(fileRoot, false);
            newbie.transform.SetAsLastSibling();
            FileItem newf = newbie.GetComponent<FileItem>();
            newf.icon.sprite = dirIco;
            newf.myPath = string.Join("\\", dir.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries));
            newf.IsDir = true;
        }

        // Then all of the files.
        foreach(string file in files) {
            GameObject newbie = Instantiate<GameObject>(ElementPrefab);
            newbie.transform.SetParent(fileRoot, false);
            newbie.transform.SetAsLastSibling();
            FileItem newf = newbie.GetComponent<FileItem>();
            newf.myPath = string.Join("\\", file.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries));
            newf.icon.sprite = defaultFile;
            foreach(Icon i in specialIcons) {
                if(newf.myPath.EndsWith(i.name)) {
                    newf.icon.sprite = i.image;
                }
            }
            newf.IsDir = false;
        }

        // Have Unity fix the layout for the file listing
        LayoutRebuilder.MarkLayoutForRebuild(fileRoot);
    }
}
