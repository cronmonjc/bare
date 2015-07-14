using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DirHide : MonoBehaviour {
    public GameObject dirListing;
    public Text[] chevrons;

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

    public void Clicked() {
        dirListing.SetActive(!dirListing.activeInHierarchy);

        string chev = (dirListing.activeInHierarchy ? "<<" : ">>");

        foreach(Text t in chevrons) {
            t.text = chev;
        }
    }
}
