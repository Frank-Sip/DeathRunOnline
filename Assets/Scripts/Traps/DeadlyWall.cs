using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadlyWall : FallingPlatform
{
    protected void Start()
    {
        initialPosition = transform.localPosition;
        fallenPosition = initialPosition - Vector3.forward * fallHeight;
    }
}
