using Google.Protobuf.Protocol;
using UnityEngine;

/* Last Modified : 24. 09. 09
 * Version : 1.011
 */

public class Card : MonoBehaviour, IAsset
{
    public int Id { get; set; }
    public UnitClass Class { get; set; }
    public Asset AssetType { get; set; }
}
