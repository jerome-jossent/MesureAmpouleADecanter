using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class decantation_simulator : MonoBehaviour
{
    float max, min, value;
    public float speed_mm_per_sec;
    public Transform tr;

    public float distance_on_screen_cm;
    public float speed_on_screen_mm_by_sec;

    float t0;

    private void Start()
    {
        tr.localScale = new Vector3(tr.localScale.x, max, tr.localScale.z);
    }

    private void OnEnable()
    {
        t0=Time.time;
    }

    void Update()
    {
        value = tr.localScale.y + speed_mm_per_sec * Time.deltaTime;

        if (value < min)
        {
            value = max;
            speed_on_screen_mm_by_sec = distance_on_screen_cm * 10 / (Time.time - t0);
            t0=Time.time;
        }

        tr.localScale = new Vector3 (tr.localScale.x, value, tr.localScale.z);
    }
}
