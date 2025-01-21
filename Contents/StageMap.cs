using UnityEngine;
using Zenject;

public class StageMap : MonoBehaviour
{
    private SinglePlayViewModel _singlePlayVm; 
    
    public int StageMapId { get; set; } 
    
    [Inject]
    public void Construct(SinglePlayViewModel singlePlayVm)
    {
        _singlePlayVm = singlePlayVm;
    }
}
