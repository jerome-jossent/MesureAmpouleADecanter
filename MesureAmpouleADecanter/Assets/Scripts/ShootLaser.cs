using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ShootLaser : MonoBehaviour
{
    public Material material;
    LaserBeam laserBeam;
    public Color color = Color.green;

    void Update()
    {
        DestroyImmediate(laserBeam?.laserObj);
        if (Lasers._instance != null)
        {
            if (Lasers._instance.run || Lasers._instance.run_once)
                UpdateLaser();
        }
    }

    private void LateUpdate()
    {
        if (Lasers._instance != null)
        {
            if (Lasers._instance.run_once)
            {
                Lasers._instance.run_once = false;
            }
        }
    }

    void UpdateLaser()
    {
        DestroyImmediate(laserBeam?.laserObj);
        laserBeam = new LaserBeam(gameObject.transform.position,
                                  gameObject.transform.right,
                                  material,
                                  color);
    }
}
