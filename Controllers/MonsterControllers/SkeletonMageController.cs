using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class SkeletonMageController : SkeletonGiantController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.SkeletonMage;
    }
}
