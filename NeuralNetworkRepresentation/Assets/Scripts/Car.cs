using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    [Header("Car settigns")]
    public float acceletationFactor = 30;
    public float turnFactor = 3.5f, driftFactor = 0.95f;
    public float maxSpeed = 4;

    private float accelerationInput = 0;
    private float steeringInput = 0;

    private float rotationAngle = 0;
    private float velocityVUP = 0;

    [NonSerialized] public Rigidbody2D rb;
    void Start()
    {
        rotationAngle = transform.localRotation.eulerAngles.z;
        rb = GetComponent<Rigidbody2D>();
    }

    public void RestartRotation()
    {
        rotationAngle = transform.localRotation.eulerAngles.z;
        velocityVUP = 0;
    }

    private void FixedUpdate()
    {
        ApplyEngineForce();
        RemoveSideForces();
        ApplySteering();
    }

    void ApplyEngineForce()
    {
        velocityVUP = Vector2.Dot( transform.up, rb.velocity);
        if (velocityVUP > maxSpeed && accelerationInput > 0)
            return;
        if (velocityVUP < -maxSpeed * 0.5 && accelerationInput < 0)
            return;
        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed && accelerationInput > 0)
            return;
        if (accelerationInput == 0)
            rb.drag = Mathf.Lerp(rb.drag, 3f, Time.fixedDeltaTime * 3);
        else
            rb.drag = 0;
        Vector2 forceVector = transform.up * accelerationInput * acceletationFactor;
        rb.AddForce(forceVector, ForceMode2D.Force);
    }

    void ApplySteering()
    {
        float minSpeedtoTurn = rb.velocity.magnitude / 2;
        minSpeedtoTurn = Mathf.Clamp01(minSpeedtoTurn);
        rotationAngle -= steeringInput * turnFactor * minSpeedtoTurn;
        rb.MoveRotation(rotationAngle);
    }

    void RemoveSideForces()
    {
        Vector2 fwVelocity = transform.up * Vector2.Dot(rb.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.velocity, transform.right);

        rb.velocity = fwVelocity + rightVelocity * driftFactor;
    }

    public void SetInputVector(Vector2 input)
    {
        steeringInput = input.x;
        accelerationInput = input.y;
    }
}
