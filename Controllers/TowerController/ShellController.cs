using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Google.Protobuf.Protocol;

public class ShellController : TowerController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Shell;
        AttackAnimValue = 2 / 3f;
    }
}
