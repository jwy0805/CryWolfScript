using UnityEngine;

public class TutorialNpcInfo : MonoBehaviour
{
    [SerializeField] public float sizeX;
    [SerializeField] public float sizeY;
    private Vector3 Offset => transform.forward * sizeX;
    public Vector3 Position => transform.position + Vector3.up * sizeY;
    public Vector3 CameraPosition => Position + Offset;
}
