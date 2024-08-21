using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class decantation_simulator : MonoBehaviour
{
    public bool run = true;
    public bool reset = false;
    float max, min;
    public float value;
    float value_prec;
    public float speed_mm_per_sec = -0.1f;
    public Transform tr;

    public float distance_on_screen_cm;

    public float info_speed_on_screen_mm_by_sec;

    float t0;

    private void Start()
    {
        min = 0.01f;
        max = tr.localScale.y;
        SetToMax();
    }

    private void OnEnable()
    {
        t0 = Time.time;
    }

    void SetToMax()
    {
        tr.localScale = new Vector3(tr.localScale.x, max, tr.localScale.z);
    }

    void Update()
    {
        if (reset)
        {
            reset = false;
            SetToMax();
        }


        if (run == false) return;


        value = tr.localScale.y + speed_mm_per_sec * Time.deltaTime;

        float delta_value = value - value_prec;

        info_speed_on_screen_mm_by_sec = delta_value / Time.deltaTime;

        if (value < min)
        {
            value = max;
            t0 = Time.time;
        }

        tr.localScale = new Vector3(tr.localScale.x, value, tr.localScale.z);
        value_prec = value;
    }
}
