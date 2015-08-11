using UnityEngine;
using System.Collections;

/// <summary>
/// Component used as a clock for the patterns.
/// </summary>
public class PattTimer : MonoBehaviour {
    /// <summary>
    /// The single instance of this Component
    /// </summary>
    public static PattTimer inst;
    /// <summary>
    /// Whether or not this clock is running
    /// </summary>
    public bool running = false;
    /// <summary>
    /// The number of passed ticks
    /// </summary>
    public ulong passedTicks = 0;
    /// <summary>
    /// The tick length, in units of 10 milliseconds
    /// </summary>
    public short tickLength = 0;
    /// <summary>
    /// How much time has passed since starting the clock
    /// </summary>
    public float passedTime = 0f;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        inst = this;
        tickLength = LightDict.inst.pattBase;
    }

    /// <summary>
    /// Resets and starts the timer.
    /// </summary>
    public void StartTimer() {
        passedTicks = 0;
        passedTime = 0f;
        running = true;
    }

    /// <summary>
    /// Stops the timer.
    /// </summary>
    public void StopTimer() {
        running = false;
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(!running) return;
        passedTime += Time.deltaTime;

        while(passedTime > (tickLength * 0.001f)) {
            passedTime -= (tickLength * 0.001f);
            passedTicks++;
        }
    }

}
