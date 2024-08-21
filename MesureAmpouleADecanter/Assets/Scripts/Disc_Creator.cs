using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]

public class Disc_Creator : MonoBehaviour
{
    public bool compute_now;

    public float h_max = 0.40f;
    public float pas = 0.01f;

    [Header("Angle du cône en degrés")]
    public float beta = 20f;
    float alpha;

    List<GameObject> discs = new List<GameObject>();

    public Color couleurLiquide;

    void Update()
    {
        if (compute_now)
        {
            compute_now = false;
            Compute();
        }
    }

    void OnValidate()
    {
        Compute();
    }

    void Compute()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            try
            {
                if (gameObject.transform.childCount > 0)
                    DestroyImmediate(gameObject.transform.GetChild(0).gameObject);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            discs.Clear();

            GameObject discs_go = new GameObject();
            if (discs_go == null) 
                return;

            discs_go.transform.parent = transform;

            alpha = beta / 2;
            for (float h = 0; h < h_max; h += pas)
            {
                float r = Compute_r(alpha, h);
                CreateDisc(h, r, discs_go.transform);
            }
        };
    }

    float Compute_r(float alpha, float h)
    {
        float r = h * MathF.Atan(alpha * MathF.PI / 180);
        return r;
    }

    void CreateDisc(float h, float r, Transform parent)
    {
        GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.transform.localScale = new Vector3(r, 0.0005f, r);
        disc.transform.localPosition = new Vector3(0, h, 0);
        disc.transform.parent = parent;
        disc.GetComponent<Renderer>().sharedMaterial.color = couleurLiquide;
        discs.Add(disc);
    }
}