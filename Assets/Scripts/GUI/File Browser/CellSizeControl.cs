using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UI Component, File Browser.  Controls the size of the cells (aka zoom) in the file listing.
/// </summary>
public class CellSizeControl : MonoBehaviour {
    /// <summary>
    /// The reference to the GridLayoutGroup
    /// </summary>
    private GridLayoutGroup grid;

    /// <summary>
    /// Gets or sets the size of a cell.
    /// </summary>
    public Vector2 CellSize {
        get {
            if(grid == null) grid = GetComponent<GridLayoutGroup>();
            return grid.cellSize;
        }
        set {
            if(grid == null) grid = GetComponent<GridLayoutGroup>();
            grid.cellSize = value;
            LayoutRebuilder.MarkLayoutForRebuild(grid.transform as RectTransform);
        }
    }

    /// <summary>
    /// Sets the size to a specified value.  Called by the zoom slider.
    /// </summary>
    public void SetSize(float to) {
        CellSize = new Vector2(48, 48) * to;
    }
}
