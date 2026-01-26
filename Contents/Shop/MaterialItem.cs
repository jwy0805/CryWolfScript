using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class MaterialItem : MonoBehaviour, IAsset
{
    public int Id { get; set; }
    public UnitClass Class { get; set; }
}
