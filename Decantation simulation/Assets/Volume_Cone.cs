using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Volume_Cone : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float volume_L;
    public Transform Cone;

    float angle_deg = 6.95f;
public    float H;
    void Update()
    {
        float angle_rad = angle_deg / 180 * Mathf.PI;

        //V = 1/3 * pi * r² * H
        // r = Mathf.Tan(angle_rad) * H;
        //V = 1/3 * pi * (Mathf.Tan(angle_rad))² * H² * H
        //H = (3*V/(pi * (Mathf.Tan(angle_rad))² ) ^ (1/3)

        H = Mathf.Pow(3 * volume_L/1000 / (Mathf.PI * Mathf.Pow(Mathf.Tan(angle_rad), 2)), 1f / 3);

        Cone.localScale = new Vector3(H, H, H);
    }
}
