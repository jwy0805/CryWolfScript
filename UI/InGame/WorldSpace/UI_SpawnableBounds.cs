using Google.Protobuf.Protocol;
using UnityEngine;

public class UI_SpawnableBounds : MonoBehaviour
{
    private float _linePos = 0f;
    private float _minZ = 0;
    private float _maxZ = 0;
    
    // UnitRole must be set before setting LinePos
    public Role UnitRole { get; set; } = Role.None;
    public float LinePos
    {
        get => _linePos;
        set
        {
            _linePos = value;
            CalcBounds(UnitRole);
        }
    }
    
    private void CalcBounds(Role unitRole)
    {
        if (Util.Faction == Faction.Sheep)
        {
            if (unitRole is Role.Tanker or Role.Warrior)
            {
                _minZ = _linePos;
                _maxZ = _linePos + 3;
            }
            else
            {
                _minZ = (GameData.MinZ + _linePos) / 2;
                _maxZ = _linePos;
            }
        }
        else
        {
            _minZ = _linePos + 3;
            _maxZ = GameData.MaxZ;
        }

        transform.position = new Vector3(0, 6.01f, _minZ + (_maxZ - _minZ) / 2);
        transform.localScale = new Vector3(220, (_maxZ - _minZ) * 10);
    }
}
