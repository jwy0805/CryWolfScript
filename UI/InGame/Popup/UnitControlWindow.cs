using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cinemachine;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public interface IUnitControlWindow
{
    void UpdateDeleteCostText(int cost);
    void UpdateRepairCostText(int cost);
    void UpdateRepairAllCostText(int cost);
}

public class UnitControlWindow : UI_Popup, IUnitControlWindow
{
    private GameViewModel _gameVm;

    private int _hp;
    private int _maxHp;
    private int _mp;
    private int _maxMp;
    private CreatureController _cc;
    private GameObject _selectedUnit;
    private TextMeshProUGUI _hpText;
    private TextMeshProUGUI _mpText;
    private Camera _portraitCamera;
    private RawImage _portraitRawImage;
    private RenderTexture _portraitRenderTexture;
    
    public GameObject SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            _selectedUnit = value;
            _cc = _selectedUnit.GetComponent<CreatureController>();
            
            // set selected portrait 
            if (_cc.UnitId != UnitId.UnknownUnit)
            {
                _gameVm.SetPortraitFromFieldUnit(_cc.UnitId);
            }
        }
    }
    
    #region Enums

    private enum Buttons
    {
        UnitDeleteButton,
        UnitRepairButton,
        UnitRepairAllButton,
        UnitSkillButton,
    }

    private enum Images
    {
        UnitDeletePanel,
        UnitRepairPanel,
        UnitRepairAllPanel,
        UnitSkillPanel,
        
        UnitDeleteButtonPanel,
        UnitRepairButtonPanel,
        UnitRepairAllButtonPanel,
        UnitSkillButtonPanel,

        UnitDeleteGoldImage,
        UnitDeleteGoldPlusImage,
        UnitRepairGoldImage,
        UnitRepairAllGoldImage,
        
        UnitPortraitFrame,
    }
    
    private enum Texts
    {
        UnitDeleteText,
        UnitRepairText,
        UnitRepairAllText,
        UnitSkillText,
        
        UnitDeleteGoldText,
        UnitRepairGoldText,
        UnitRepairAllGoldText,
        HpText,
        MpText
    }

    #endregion
    
    [Inject]
    public void Construct(GameViewModel gameViewModel)
    {
        _gameVm = gameViewModel;
    }

    protected override async void Init()
    {
        try
        {
            base.Init();
            _gameVm.UnitControlWindow = this;

            BindObjects();
            await InitUIAsync();
            InitButtonEvents();
            InitCamera();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    protected void FixedUpdate()
    {
        if (_cc == null || _portraitCamera == null) return;
        
        var offset = _cc.transform.forward * _cc.Stat.SizeX;
        _portraitCamera.transform.position = _cc.transform.position + offset + Vector3.up * _cc.Stat.SizeY;
        
        if (_hp == _cc.Hp && _maxHp == _cc.MaxHp && _mp == _cc.Mp && _maxMp == _cc.MaxMp) return;
        _hpText.text = $"{_cc.Hp} / {_cc.MaxHp}";
        _mpText.text = _maxMp is 0 or 1 ? "- / -" : $"{_cc.Mp} / {_cc.MaxMp}";
        _hp = _cc.Hp;
        _maxHp = _cc.MaxHp;
        _mp = _cc.Mp;
        _maxMp = _cc.MaxMp;
        
        var hpRatio = _cc.Hp / (float)_cc.MaxHp * 100;
        _hpText.color = hpRatio switch
        {
            > 70.0f => Color.green,
            < 30.0f => Color.red,
            _ => Color.yellow
        };
    }

    private void InitCamera()
    {
        _portraitCamera = GameObject.FindGameObjectsWithTag("Camera")
            .FirstOrDefault(obj => obj.name == "PortraitCam")?.GetComponent<Camera>();
        
        if (_portraitCamera != null && _cc != null)
        {
            var unitPortraitFrame = GetImage((int)Images.UnitPortraitFrame);
            _portraitRawImage = unitPortraitFrame.GetComponentInChildren<RawImage>();
            _portraitRenderTexture = Managers.Resource.CreateRenderTexture("portraitTexture");
            
            _portraitRawImage.texture = _portraitRenderTexture;
            _portraitCamera.targetTexture = _portraitRenderTexture;

            if (_cc.TryGetComponent(out Collider col))
            {
                var bounds = col.bounds;

                // 콜라이더 가장 위 점과 높이
                float height = bounds.size.y;                          // 전체 높이
                float width = Math.Max(bounds.size.x, bounds.size.z); // 가로, 세로 중 큰 값
                Vector3 top = bounds.center + Vector3.up * bounds.extents.y;

                // 높이의 20% 만큼 아래로 내린 점 A
                float offsetH = height * 0.2f;
                Vector3 target = top - Vector3.up * offsetH;

                // 카메라 위치: A에서 내려간 거리만큼 정면 쪽으로 이동
                Vector3 viewDir = _cc.transform.forward.normalized;   // 필요하면 여기 + 로 바꿔도 됨
                Vector3 camPos = top + viewDir * width + Vector3.forward;

                _portraitCamera.transform.position = camPos;
                _portraitCamera.transform.LookAt(target);
                
                Debug.Log($"{height} {bounds.extents.y}");
            }
        }
    }
    
    protected override async Task InitUIAsync()
    {
        var images = new List<Images>();
        switch (_cc.ObjectType)
        {
            case GameObjectType.Tower:
                images.AddRange(new[] { Images.UnitDeletePanel, Images.UnitSkillPanel });
                break;
            case GameObjectType.Fence:
                images.AddRange(new[] { Images.UnitRepairPanel, Images.UnitRepairAllPanel });
                break;
            case GameObjectType.Sheep:
                images.AddRange(new[] { Images.UnitSkillPanel });
                break;
            case GameObjectType.Monster:
                images.AddRange(new[] { Images.UnitSkillPanel });
                break;
            case GameObjectType.MonsterStatue:
                images.AddRange(new[]
                {
                    Images.UnitDeletePanel,
                    Images.UnitRepairPanel,
                    Images.UnitSkillPanel
                });
                break;
        }
        
        BindControlButtons(images);
        
        var goldIconPath = Util.Faction == Faction.Sheep ? "Sprites/UIIcons/icon_coin" : "Sprites/UIIcons/icon_dna";

        var unitDeleteText = GetText((int)Texts.UnitDeleteText);
        var unitRepairText = GetText((int)Texts.UnitRepairText);
        var unitRepairAllText = GetText((int)Texts.UnitRepairAllText);
        var unitSkillText = GetText((int)Texts.UnitSkillText);
        
        GetImage((int)Images.UnitDeleteGoldImage).sprite = await Managers.Resource.LoadAsync<Sprite>(goldIconPath);
        GetImage((int)Images.UnitRepairGoldImage).sprite = await Managers.Resource.LoadAsync<Sprite>(goldIconPath);
        GetImage((int)Images.UnitRepairAllGoldImage).sprite = await Managers.Resource.LoadAsync<Sprite>(goldIconPath);
        
        await Managers.Localization.BindLocalizedText(unitDeleteText, "delete_text");
        await Managers.Localization.BindLocalizedText(unitRepairText, "repair_text");
        await Managers.Localization.BindLocalizedText(unitRepairAllText, "repair_all_text");
        await Managers.Localization.BindLocalizedText(unitSkillText, "skill_tree_text");
    }

    private void BindControlButtons(List<Images> images)
    {
        var allImages = new List<Images>
        {
            Images.UnitDeletePanel,
            Images.UnitRepairPanel,
            Images.UnitRepairAllPanel,
            Images.UnitSkillPanel
        };
        
        var imagesToBeHidden = allImages.Except(images).ToList();
        foreach (var hiddenImage in imagesToBeHidden)
        {
            GetImage((int)hiddenImage).gameObject.SetActive(false);
        }
        
        // Set the position of the images by the number of images (The number of images is different for each object type)
        Image image;
        float increment;
        switch (images.Count)
        {
            case 1:
                image = GetImage((int)images[0]);
                image.GetComponent<RectTransform>().anchorMin = new Vector2(0.3f, 0f);
                image.GetComponent<RectTransform>().anchorMax = new Vector2(0.7f, 1f);
                break;
            case 2:
                increment = 0.4f;
                for (var i = 0; i < images.Count; i++)
                {
                    image = GetImage((int)images[i]);
                    image.GetComponent<RectTransform>().anchorMin = new Vector2(0.1f + increment * i, 0f);
                    image.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f + increment * i, 1f);
                }
                break;
            case 3:
                increment = 0.33f;
                for (var i = 0; i < images.Count; i++)
                {
                    image = GetImage((int)images[i]);
                    image.GetComponent<RectTransform>().anchorMin = new Vector2(0f + increment * i, 0f);
                    image.GetComponent<RectTransform>().anchorMax = new Vector2(0.33f + increment * i, 1f);
                }
                break;
            case 4:
                increment = 0.25f;
                for (var i = 0; i < images.Count; i++)
                {
                    image = GetImage((int)images[i]);
                    image.GetComponent<RectTransform>().anchorMin = new Vector2(0f + increment * i, 0f);
                    image.GetComponent<RectTransform>().anchorMax = new Vector2(0.25f + increment * i, 1f);
                }
                break;
        }
        
        _gameVm.UpdateUnitDeleteCostRequired(new []{ _cc.Id });
        _gameVm.UpdateUnitRepairCostRequired(new []{ _cc.Id });
    }
    
    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        _hpText = GetText((int)Texts.HpText);
        _mpText = GetText((int)Texts.MpText);
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.UnitDeleteButton).gameObject.BindEvent(OnDeleteClicked);
        GetButton((int)Buttons.UnitRepairButton).gameObject.BindEvent(OnRepairClicked);
        GetButton((int)Buttons.UnitRepairAllButton).gameObject.BindEvent(OnRepairAllClicked);
        GetButton((int)Buttons.UnitSkillButton).gameObject.BindEvent(OnSkillClicked);
    }
    
    public void UpdateDeleteCostText(int cost)
    {
        GetText((int)Texts.UnitDeleteGoldText).text = cost.ToString();
    }
    
    public void UpdateRepairCostText(int cost)
    {
        GetText((int)Texts.UnitRepairGoldText).text = cost.ToString();
    }
    
    public void UpdateRepairAllCostText(int cost)
    {
        GetText((int)Texts.UnitRepairAllGoldText).text = cost.ToString();
    }
    
    private void OnDeleteClicked(PointerEventData data)
    {
        if (_cc == null) return;
        _gameVm.OnUnitDeleteClicked(new[] { _cc.Id });
    }
    
    private void OnRepairClicked(PointerEventData data)
    {
        if (_cc == null) return;
        _gameVm.OnUnitRepairClicked(new[] { _cc.Id });
    }
    
    private void OnRepairAllClicked(PointerEventData data)
    {
        _gameVm.OnUnitRepairAllClicked();
    }

    private async Task OnSkillClicked(PointerEventData data)
    {
        await _gameVm.OnUnitSkillClicked();
    }

    private void OnDestroy()
    {
        _gameVm.UnitControlWindow = null;
    }
}
