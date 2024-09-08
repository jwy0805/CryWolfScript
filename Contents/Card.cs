using Google.Protobuf.Protocol;
using UnityEngine;

public class Card : MonoBehaviour, IAsset
{
    public int Id { get; set; }
    public UnitClass Class { get; set; }
}
