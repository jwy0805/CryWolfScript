using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class Stage : MonoBehaviour
{
    [SerializeField] public int stageId;
    
    private SinglePlayViewModel _singlePlayVm;

    public Faction Faction => Util.Faction;
    public List<UserStageInfo> UserStageInfo { get; set; } = new();
    public List<UnitInfo> EnemyInfo { get; set; } = new();
    public List<SingleRewardInfo> RewardInfo { get; set; } = new();
    
    [Inject]
    public void Construct(SinglePlayViewModel singlePlayVm)
    {
        _singlePlayVm = singlePlayVm;
    }

    private void Start()
    {
        FindAnyObjectByType<SceneContext>().Container.Inject(this);
        Init();
    }

    private void Init()
    {
        BindObjects();
        InitUI();
    }
    
    private void BindObjects()
    {
        UserStageInfo = _singlePlayVm.UserStageInfos;

        var unitIds = _singlePlayVm.StageEnemyInfos.FirstOrDefault(sei => sei.StageId == stageId)?.UnitIds;
        if (unitIds != null)
        {
            EnemyInfo = Managers.Data.UnitInfoDict.Values.Where(ui => unitIds.Contains((UnitId)ui.Id)).ToList();
        }
        
        RewardInfo = _singlePlayVm.StageRewardInfos.FirstOrDefault(sri => sri.StageId == stageId)?.RewardProducts;
    }

    private void InitUI()
    {
        if (UserStageInfo.Select(usi => usi.StageId).Contains(stageId))
        {
            var stageStars = UserStageInfo.FirstOrDefault(usi => usi.StageId == stageId)?.StageStar;
            if (stageStars != null)
            {
                var stars = stageStars.Value;
                var starPanel = transform.Find("Star");
                for (var i = 0; i < 3; i++)
                {
                    starPanel.GetChild(i).gameObject.SetActive(i < stars);
                }
            }
            GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("Sprites/UIIcons/stage_cleared");
        }
        else
        {
            GetComponent<Image>().sprite = 
                Managers.Resource.Load<Sprite>(UserStageInfo.Max(usi => usi.StageId) + 1 == stageId 
                    ? "Sprites/UIIcons/stage_unlocked" 
                    : "Sprites/UIIcons/stage_locked");
            
            var starPanel = transform.Find("Star");
            for (var i = 0; i < 3; i++)
            {
                starPanel.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
