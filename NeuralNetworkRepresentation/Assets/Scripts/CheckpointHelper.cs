using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointHelper : MonoBehaviour
{
    private int lastCheckpoint = 0;
    private float totalCheckpoint = 0;
    private int collisions = 0;
    private Vector2 lastPos;
    private bool better;
    private float elapsedPos = 0;
    private void Start()
    {
        ResetCar();
  
    }

    public void ResetCar()
    {
        lastPos = transform.position;
        better = false;
        totalCheckpoint = 0;
        lastCheckpoint  = collisions = 0;
    }
    private void Update()
    {
        elapsedPos += Vector2.Distance(transform.position, lastPos);
        lastPos = transform.position;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Checkpoint"))
        {
            int checkpointNmbr = Int32.Parse(collision.gameObject.name);
            if(checkpointNmbr == 0)
            {
                if(lastCheckpoint == ChildRenamer.lastChildIndex)
                {
                    lastCheckpoint = 0;
                    elapsedPos = 0;
                    totalCheckpoint+=1;
                    better = false;
                }
                return;
            }

            if(checkpointNmbr == lastCheckpoint + 1)
            {
                lastCheckpoint++;
                totalCheckpoint+=1;
                elapsedPos = 0;
                better = false;
                //Debug.Log("Total chefckpoints " + totalCheckpoint);
            }

        }

        if (collision.CompareTag("BetterCheckpoint"))
        {
            if (!better)
            {
                totalCheckpoint+= 0.5f;
                better = true;
            }
        }
    }

    public float GetTotalPoints()
    {
        return totalCheckpoint > 0 ? totalCheckpoint : -100;
    }

    public float CalculateScore()
    {
        //Debug.Log(elapsedPos);
        var nextCheck = ChildRenamer.Instance.GetNearestCheckpoint(lastCheckpoint); ;

        float distance = Vector2.Distance(nextCheck.position, transform.position);
        //Debug.Log(distance);
        return GetTotalPoints() + Mathf.Clamp01(1-distance);
    }

   
}
