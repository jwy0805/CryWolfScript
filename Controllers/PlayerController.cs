using Google.Protobuf.Protocol;
using UnityEngine;

public class PlayerController : BaseController
{
    public Faction Faction { get; set; }

    public void SetPosition()
    {
        transform.position = new Vector3(PosInfo.PosX, PosInfo.PosY, PosInfo.PosZ);
        transform.rotation = Quaternion.Euler(0, PosInfo.Dir, 0);
    }
}
