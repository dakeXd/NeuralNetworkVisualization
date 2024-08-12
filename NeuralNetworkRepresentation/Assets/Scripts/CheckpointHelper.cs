using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointHelper : MonoBehaviour
{
    private int lastCheckpoint = 0;
    private int totalCheckpoint = 0;
    private int collisions = 0;
    private Vector2 lastPos;
    private float elapsedPos = 0;
    private void Start()
    {
        lastPos = transform.position;
        lastCheckpoint =  totalCheckpoint = collisions= 0;
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
                    totalCheckpoint++;
                }
                return;
            }

            if(checkpointNmbr == lastCheckpoint + 1)
            {
                lastCheckpoint++;
                totalCheckpoint++;
                elapsedPos = 0;
                //Debug.Log("Total chefckpoints " + totalCheckpoint);
            }

        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
        {
            collisions++;
        }
    }

    public int GetTotalPoints()
    {
        return totalCheckpoint > 0 ? totalCheckpoint : -100;
    }

    public float CalculateScore()
    {
        //Debug.Log(elapsedPos);
        return GetTotalPoints() + Mathf.Clamp01(elapsedPos);
    }

   
}
