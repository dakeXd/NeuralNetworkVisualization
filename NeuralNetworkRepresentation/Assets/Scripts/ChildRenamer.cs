using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildRenamer : MonoBehaviour
{
    string subfix = "";
    public static int lastChildIndex;
    // Start is called before the first frame update
    public static ChildRenamer Instance;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        var childs = transform.childCount;
        lastChildIndex = childs-1;
        for (int i = 0; i < childs; i++)
        {
            transform.GetChild(i).name = subfix + i;
        }
    }

    public Transform GetNearestCheckpoint(int nearest)
    {

        return transform.GetChild(nearest >= transform.childCount - 1 ? 0 : (nearest + 1));
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
