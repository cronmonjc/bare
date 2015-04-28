using UnityEngine;
using System.Collections.Generic;
using fNbt;

public class LightHeadDefinition {
    public OpticNode optic;
    public StyleNode style;

    public Dictionary<Function, Pattern> patterns;

    public LightHeadDefinition() {
        patterns = new Dictionary<Function, Pattern>();
    }
}