using UnityEngine;
using System.Collections.Generic;
using fNbt;

public class LightHeadDefinition {
    public OpticNode optic;
    public StyleNode style;

    public List<BasicFunction> funcs;

    public LightHeadDefinition() {
        funcs = new List<BasicFunction>();
    }
}