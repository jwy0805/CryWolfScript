using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public interface ISkillObserver
{
    void OnSkillUpdated(int id, GameObjectType type, SkillType skillType, int step);
}
