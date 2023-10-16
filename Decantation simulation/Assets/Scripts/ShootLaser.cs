using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootLaser : MonoBehaviour
{
    public Material material;
    LaserBeam laserBeam;
    public Color color = Color.green;

    void Update()
    {
        Destroy(laserBeam?.laserObj);
        laserBeam = new LaserBeam(gameObject.transform.position,
                                  gameObject.transform.right,
                                  material, 
                                  color);

    }
}
