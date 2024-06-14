using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/DataSet", order = 1)]
public class DataSetCafe : ScriptableObject
{
    public List<CafeData> data;
}

[Serializable]
public class CafeData
{
    public float  temperatura;
    public float  altitud;
    public bool valid;

    public double[] ExpectedOuput()
    {
        return valid ? new double[] { 1, 0 } : new double[] { 0, 1};
    } 
    
    public double[] Input()
    {
        return new double[] { temperatura, altitud };
    } 
}
