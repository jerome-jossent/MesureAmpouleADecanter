using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LaserBeam
{
    Vector3 pos, dir;

    public GameObject laserObj;
    LineRenderer laser;
    List<Vector3> laserIndices = new List<Vector3>();

    public LaserBeam(Vector3 pos, Vector3 dir, Material material, Color color)
    {
        laser = new LineRenderer();
        laserObj = new GameObject();
        laserObj.name = "Laser Beam";
        this.pos = pos;
        this.dir = dir;

        laser = laserObj.AddComponent<LineRenderer>();
        laser.startWidth = laser.endWidth = 0.001f;
        laser.material = material;
        laser.startColor = laser.endColor = color;

        CastRay(pos, dir, laser);
    }

    private void CastRay(Vector3 pos, Vector3 dir, LineRenderer laser)
    {
        laserIndices.Add(pos);
        
        Ray ray = new Ray(pos,dir);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 30, 1))
            CheckHit(hit, dir, laser);
        else
            laserIndices.Add(ray.GetPoint(30));
        
        UpdateLaser();
    }


    void UpdateLaser()
    {
        int count = 0;
        laser.positionCount = laserIndices.Count;

        foreach (Vector3 idx in laserIndices)
        {
            laser.SetPosition(count, idx);
            count++;
        }
    }
    private void CheckHit(RaycastHit  hitInfo, Vector3 direction, LineRenderer laser)
    {
        if(hitInfo.collider.gameObject.tag == "Mirror") 
        {
            Vector3 pos = hitInfo.point;
            Vector3 dir = Vector3.Reflect(direction, hitInfo.normal);

            CastRay(pos, dir, laser);
        }
        else
        {
            laserIndices.Add(hitInfo.point);
            UpdateLaser();
        }
    }
}
