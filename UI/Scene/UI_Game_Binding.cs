using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public partial class UI_Game
{
    #region UIDictionaray
    
    protected readonly Dictionary<string, GameObject> DictCommonBtn = new();
    protected readonly Dictionary<string, GameObject> DictCommonImg = new();
    protected readonly Dictionary<string, GameObject> DictCommonTxt = new();
    protected readonly Dictionary<string, GameObject> DictUnitBtn = new();
    protected readonly Dictionary<string, GameObject> DictControlBtn = new();
    
    // 활성화될 오브젝트
    private readonly Dictionary<string, GameObject> _dictBtn = new();
    private readonly Dictionary<string, GameObject> _dictImg = new();
    private Dictionary<string, GameObject> _dictTxt = new();
    private readonly Dictionary<string, GameObject> _dictSkillPanel = new();
    protected readonly Dictionary<string, GameObject> DictSkillBtn = new();
    private readonly Dictionary<string, GameObject> _dictLine = new();
    
    #endregion
    
    protected override void BindObjects()
    {
        if (gameObject.name == "UI_GameDoubleWay")
        {
            BindData<Button>(typeof(CommonButtonsD), DictCommonBtn);
            BindData<Image>(typeof(CommonImagesD), DictCommonImg);
            BindData<TextMeshProUGUI>(typeof(CommonTextsD), DictCommonTxt);
            BindData<Button>(typeof(UnitButtonsD), DictUnitBtn);
            BindData<Button>(typeof(UnitControlButtonsD), DictControlBtn);
        }
        else
        {
            BindData<Button>(typeof(CommonButtonsS), DictCommonBtn);
            BindData<Image>(typeof(CommonImagesS), DictCommonImg);
            BindData<TextMeshProUGUI>(typeof(CommonTextsS), DictCommonTxt);
            BindData<Button>(typeof(UnitButtonsS), DictUnitBtn);
            BindData<Button>(typeof(UnitControlButtonsS), DictControlBtn);
        }
        
        SetLog();               // 가져온 덱에 따라 통나무 UI 세팅
        BringSkillPanels();     // 가져온 덱에 따라 스킬 패널 미리 가져오기
        BringBaseSkillPanels();
    }

    protected void SetLog()
    {   // Set the selected units in the main lobby to the log bar
        for (int i = 0; i < Util.Deck.UnitsOnDeck.Length; i++)
        {
            var prefab = Managers.Resource.Instantiate(
                "UI/InGame/Portrait", DictCommonImg[$"UnitPanel{i}"].transform);
            var level = Util.Deck.UnitsOnDeck[i].Level;
            var initPortraitId = (int)Util.Deck.UnitsOnDeck[i].Id - (level - 1);
            var portrait = prefab.GetComponent<UI_Portrait>();
            portrait.UnitId = (UnitId)initPortraitId;
            prefab.GetComponent<Image>().sprite = 
                Managers.Resource.Load<Sprite>($"Sprites/Portrait/{portrait.UnitId}");
            prefab.GetComponent<Button>().onClick.AddListener(OnPortraitClicked);
        }
    }
    
    protected void BringSkillPanels()
    {
        foreach (var unit in Util.Deck.UnitsOnDeck)
        {
            for (int i = 0; i < unit.Level; i++)
            {
                var unitId = (int)unit.Id - i;
                var unitName = ((UnitId)unitId).ToString();
                var skillPanel = Managers.Resource.Instantiate($"UI/InGame/{unitName}SkillPanel");
                skillPanel.transform.SetParent(DictCommonImg["SkillPanel"].transform);
                _dictSkillPanel.Add($"{unitName}SkillPanel", skillPanel);
                SetSkillButtons(skillPanel);
            }
        }
    }

    protected void BringBaseSkillPanels()
    {
        SetBaseSkillPanelByCamp(Camp == Camp.Sheep ? Camp.Sheep : Camp.Wolf, "UI/InGame");
    }

    private void SetBaseSkillPanelByCamp(Camp camp, string path)
    {
        var panel = Managers.Resource.Instantiate(path + $"/{camp.ToString()}BaseSkillPanel");
        panel.transform.SetParent(DictCommonImg["SubResourceWindow"].transform);
        SetBaseSkillButtons(panel);
    }

    private void SetSkillButtons(GameObject go)
    {
        var skillButtons = go.GetComponentsInChildren<Button>();
        foreach (var skillButton in skillButtons)
        {
            skillButton.onClick.AddListener(OnSkillClicked);
            DictSkillBtn.Add(skillButton.name, skillButton.gameObject);
            SetObjectSize(skillButton.gameObject, 0.22f);
            Util.SetAlpha(skillButton.gameObject.GetComponent<Image>(), 0.6f);
        }
    }

    private void SetBaseSkillButtons(GameObject go)
    {
        var skillButtons = go.GetComponentsInChildren<Button>();
        foreach (var skillButton in skillButtons)
        {
            skillButton.onClick.AddListener(OnSkillClicked);
            DictSkillBtn.Add(skillButton.name, skillButton.gameObject);
            SetObjectSize(skillButton.gameObject, 0.3f);
        }
    }

    private void DeactivateUI(Dictionary<string, GameObject> dict)
    {
        foreach (var item in dict)
        {
            item.Value.gameObject.SetActive(false);
        }
    }

    protected void BindData<T>(Type enumType, Dictionary<string, GameObject> dict) where T : Object
    {
        Bind<T>(enumType);
        
        if (typeof(T) == typeof(Button))
        {
            for (int i = 0; i < Objects[typeof(T)].Length; i++)
            {
                GameObject btn = GetButton(i).gameObject;
                dict.Add(btn.name, btn);
            }
        }
        else if (typeof(T) == typeof(Image))
        {
            for (int i = 0; i < Objects[typeof(T)].Length; i++)
            {
                GameObject img = GetImage(i).gameObject;
                dict.Add(img.name, img);
            }
        }
        else if (typeof(T) == typeof(TextMeshProUGUI))
        {
            for (int i = 0; i < Objects[typeof(T)].Length; i++)
            {
                GameObject txt = GetText(i).gameObject;
                dict.Add(txt.name, txt);
            }
        }
        
        Objects.Clear();
    }
    
    public void SetSkillPanel(GameObject portrait, bool activate)
    {
        portrait.TryGetComponent(out UI_Portrait uiPortrait);
        if (_dictSkillPanel.ContainsKey($"{uiPortrait.UnitId.ToString()}SkillPanel"))
            _dictSkillPanel[$"{uiPortrait.UnitId.ToString()}SkillPanel"].SetActive(activate);
    }
    
    public void SetUpgradeButton(GameObject portrait)
    {
        var level = GetLevelFromUIObject(portrait);
        var tf = DictCommonBtn["UpgradeButton"].transform;
        var btn = tf.GetComponent<Button>();
        var go = Util.FindChild(tf.gameObject, "GoldPanel", true, true);

        if (level == 3)
        {
            btn.interactable = false;
            go.SetActive(false);
        }
        else
        {
            btn.interactable = true;
            go.SetActive(true);
            var unitId = (int)portrait.GetComponent<UI_Portrait>().UnitId;
            C_SetUpgradeButton upgradePacket = new() { UnitId = unitId };
            Managers.Network.Send(upgradePacket);
        }
    }

    public void SetUpgradeButtonText(int cost)
    {
        var btn = DictCommonBtn["UpgradeButton"];
        btn.gameObject.SetActive(true);
        var txt = Util.FindChild(btn, "GoldText", true, true).GetComponent<TextMeshProUGUI>();
        txt.text = cost.ToString();
    }
    
    protected virtual void SetTexts()
    {
        DictCommonTxt["ResourceText"].GetComponent<TextMeshProUGUI>().text = "0";
    }
    
    protected override void SetUI()
    {
        SetPanel();
        
        DictCommonImg["SkillWindow"].SetActive(false);
        DictCommonImg["CapacityWindow"].SetActive(false);
        DictCommonImg["SubResourceWindow"].SetActive(false);
        DictCommonImg["UnitControlWindow"].SetActive(false);
        
        SetObjectSize(DictCommonBtn["UpgradeButton"], 0.95f); 
    }
    
    private void SetPanel()
    {
        foreach (var value in DictUnitBtn.Values)
        {
            SetObjectSize(value.transform.parent.parent.gameObject, 0.4f);
        }

        foreach (var value in DictControlBtn.Values)
        {
            SetObjectSize(value.transform.parent.parent.gameObject, 0.3f);
        }
        
        foreach (var pair in _dictSkillPanel)
        {
            pair.Value.TryGetComponent(out RectTransform rt);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0, 0);
            pair.Value.SetActive(false);
        }
    }
    
    private int GetLevelFromUIObject(GameObject go)
    {
        go.TryGetComponent(out UI_Portrait uiPortrait);
        var unitId = uiPortrait.UnitId;
        var level = ((int)unitId % 100) % 3;
        if (level == 0) level = 3;
        return level;
    }
    
    public void SetMenu(bool activate)
    {
        DictCommonImg["MenuItemPanel"].SetActive(activate);
    }
}
