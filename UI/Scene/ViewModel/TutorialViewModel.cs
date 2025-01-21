using System;
using UnityEngine;

public class TutorialViewModel
{
    public event Action<Vector3, Vector3> OnInitTutorialCamera;
    
    public void InitTutorialCamera(Vector3 npcPosition, Vector3 cameraPosition)
    {
        OnInitTutorialCamera?.Invoke(npcPosition, cameraPosition);
    }
}
