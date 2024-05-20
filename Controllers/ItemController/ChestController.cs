using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;

public class ChestController : ResourceController
{
    public override State State
    {
        get => PosInfo.State;
        set
        {
            PosInfo.State = value;
            switch (PosInfo.State)
            {
                case State.Moving:
                    Anim.CrossFade("RUN", 0.1f, 0);
                    break;
            }
        }
    }
}
