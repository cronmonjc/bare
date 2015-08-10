using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UI Component, File Browser.  Small class that allows for hiding the directory listing if desired.
/// </summary>
public class DirHide : MonoBehaviour {
    /// <summary>
    /// The directory listing reference.  Set via Unity Inspector.
    /// </summary>
    public GameObject dirListing;
    /// <summary>
    /// The references to the chevron Text Components.  Set via Unity Inspector.
    /// </summary>
    public Text[] chevrons;

    /// <summary>
    /// Gets or sets a value indicating whether the directory listing is shown.
    /// </summary>
    public bool DirShown {
        get { return dirListing.activeInHierarchy; }
        set {
            dirListing.SetActive(value);

            string chev = (value ? "<<" : ">>");

            foreach(Text t in chevrons) {
                t.text = chev;
            }
        }
    }

    /// <summary>
    /// Called when the user clicks the show/hide button.  Toggles whether the listing is visible.
    /// </summary>
    public void Clicked() {
        dirListing.SetActive(!dirListing.activeInHierarchy);

        string chev = (dirListing.activeInHierarchy ? "<<" : ">>");

        foreach(Text t in chevrons) {
            t.text = chev;
        }
    }
}
