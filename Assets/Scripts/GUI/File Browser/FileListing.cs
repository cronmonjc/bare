using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class FileListing : MonoBehaviour {
    public Sprite defaultFile, dirIco;
    [System.Serializable]
    public struct Icon {
        public string name;
        public Sprite image;
    }
    public Icon[] specialIcons;

    public RectTransform fileRoot;
    public GameObject ElementPrefab;

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

    public void Refresh() {
        Clear();

        FileBrowser fb = transform.parent.GetComponent<FileBrowser>();
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
            newf.myPath = string.Join("/", dir.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries));
            newf.IsDir = true;
        }

        // Then all of the files.
        foreach(string file in files) {
            GameObject newbie = Instantiate<GameObject>(ElementPrefab);
            newbie.transform.SetParent(fileRoot, false);
            newbie.transform.SetAsLastSibling();
            FileItem newf = newbie.GetComponent<FileItem>();
            newf.myPath = string.Join("/", file.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries));
            newf.icon.sprite = defaultFile;
            foreach(Icon i in specialIcons) {
                if(newf.myPath.EndsWith(i.name)) {
                    newf.icon.sprite = i.image;
                }
            }
            newf.IsDir = false;
        }

        LayoutRebuilder.MarkLayoutForRebuild(fileRoot);
    }
}
