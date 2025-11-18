using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadlyWall : FallingPlatform
{
    [Header("Deadly Wall Direction")]
    [SerializeField] private bool moveRight;
    [SerializeField] private bool moveBack;
    [SerializeField] private bool moveLeft;
    [SerializeField] private bool moveForward;

    protected void Start()
    {
        initialPosition = transform.localPosition;
        Vector3 direction = Vector3.forward; // Default
        if (moveRight) direction = Vector3.right;
        else if (moveBack) direction = Vector3.back;
        else if (moveLeft) direction = Vector3.left;
        else if (moveForward) direction = Vector3.forward;
        fallenPosition = initialPosition + direction * fallHeight;
    }
}
