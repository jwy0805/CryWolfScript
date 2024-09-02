using System;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;

public interface ISkillButton
{
    string Name { get; }
}

public class UI_Skill : MonoBehaviour, ISkillButton
{
    public string Name => gameObject.name;
}
