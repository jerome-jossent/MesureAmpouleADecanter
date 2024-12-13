using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class Lasers : MonoBehaviour
{
    public bool run;
    public bool run_once;

    public static Lasers _instance;
    
    
    private void Awake()
    {
        _instance = this; 
    }
}
