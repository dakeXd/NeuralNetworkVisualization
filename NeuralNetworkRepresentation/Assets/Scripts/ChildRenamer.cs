using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildRenamer : MonoBehaviour
{
    string subfix = "";
    public static int lastChildIndex;
    // Start is called before the first frame update
    void Awake()
    {
        var childs = transform.childCount;
        lastChildIndex = childs-1;
        for (int i = 0; i < childs; i++)
        {
            transform.GetChild(i).name = subfix + i;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
