using UnityEngine;
using System.Collections.Generic;
using fNbt;

/// <summary>
/// The definition of a LightHead.  Holds the physical information of a head.
/// </summary>
public class LightHeadDefinition {
    /// <summary>
    /// The optic the LightHead's using
    /// </summary>
    public OpticNode optic;
    /// <summary>
    /// The style the LightHead's using
    /// </summary>
    public StyleNode style;

    /// <summary>
    /// The functions the LightHead's using
    /// </summary>
    public List<BasicFunction> funcs;

    public LightHeadDefinition() {
        funcs = new List<BasicFunction>();
    }
}