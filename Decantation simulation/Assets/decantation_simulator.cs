using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class decantation_simulator : MonoBehaviour
{
    public float max, min, value;
    public float speed_mm_per_sec;
    public Transform tr;

    private void Start()
    {
        tr.localScale = new Vector3(tr.localScale.x, max, tr.localScale.z);
    }

    void Update()
    {
        value = tr.localScale.y + speed_mm_per_sec * Time.deltaTime;
     
        if (value < min) value = max;

        tr.localScale = new Vector3 (tr.localScale.x, value, tr.localScale.z);        
    }
}
