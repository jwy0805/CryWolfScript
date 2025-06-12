using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Google.Protobuf.Protocol;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class Stage : MonoBehaviour
{
    [SerializeField] public int stageId;
    
    private SinglePlayViewModel _singlePlayVm;

    public Faction Faction => Util.Faction;
    [CanBeNull] public UserStageInfo UserStageInfo { get; set; }
    public List<UnitInfo> EnemyInfo { get; set; } = new();
    public List<SingleRewardInfo> RewardInfo { get; set; } = new();
    
    [Inject]
    public void Construct(SinglePlayViewModel singlePlayVm)
    {
        _singlePlayVm = singlePlayVm;
    }

    private void Start()
    {
        Util.Inject(this);
        Init();
    }

    private void Init()
    {
        BindObjects();
        InitUI();
    }
    
    private void BindObjects()
    {
        UserStageInfo = _singlePlayVm.UserStageInfos.FirstOrDefault(usi => usi.StageId == stageId);

        var unitIds = _singlePlayVm.StageEnemyInfos.FirstOrDefault(sei => sei.StageId == stageId)?.UnitIds;
        if (unitIds != null)
        {
            EnemyInfo = Managers.Data.UnitInfoDict.Values.Where(ui => unitIds.Contains((UnitId)ui.Id)).ToList();
        }
        
        RewardInfo = _singlePlayVm.StageRewardInfos.FirstOrDefault(sri => sri.StageId == stageId)?.RewardProducts;
    }

    private void InitUI()
    {
        var starPanel = transform.Find("Star");
        if (UserStageInfo == null)
        {
            GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("Sprites/UIIcons/stage_locked");
            for (var i = 0; i < 3; i++)
            {
                starPanel.GetChild(i).gameObject.SetActive(false);
            }
        }
        else
        {
            GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(
                UserStageInfo.IsCleared ? "Sprites/UIIcons/stage_cleared" : "Sprites/UIIcons/stage_unlocked");
            var stars = UserStageInfo.StageStar;
            for (var i = 0; i < 3; i++)
            {
                starPanel.GetChild(i).gameObject.SetActive(i < stars);
            }
        }
    }
}
