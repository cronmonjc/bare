using UnityEngine;
using System.Collections;

public class PattTimer : MonoBehaviour {
    public static PattTimer inst;
    public bool running = false;
    public ulong passedTicks = 0;
    public short tickLength = 0;
    public float passedTime = 0f;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        inst = this;
        tickLength = LightDict.inst.pattBase;
    }

    public void StartTimer() {
        passedTicks = 0;
        passedTime = 0f;
        running = true;
    }

    public void StopTimer() {
        running = false;
    }

    void Update() {
        if(!running) return;
        passedTime += Time.deltaTime;

        while(passedTime > (tickLength * 0.001f)) {
            passedTime -= (tickLength * 0.001f);
            passedTicks++;
        }
    }

}
