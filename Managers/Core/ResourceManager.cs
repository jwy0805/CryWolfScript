using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

public class ResourceManager
{
    public T Load<T>(string path) where T : Object
    {
        if (typeof(T) == typeof(GameObject))
        {
            string name = path;
            int index = name.LastIndexOf('/');
            if (index >= 0)
                name = name.Substring(index + 1);

            GameObject go = Managers.Pool.GetOriginal(name);
            if (go != null)
                return go as T;
        }
        return Resources.Load<T>(path);
    }

    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        
        if (original == null)
        {
            Debug.Log($"Failed to load Prefab : {path}");
            return null;
        }

        if (original.GetComponent<Poolable>() != null)
            return Managers.Pool.Pop(original, parent).gameObject;

        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        return go;
    }   
    
    public GameObject Instantiate(GameObject original, Transform parent = null)
    {
        if (original.GetComponent<Poolable>() != null)
            return Managers.Pool.Pop(original, parent).gameObject;

        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        return go;
    }
    
    public GameObject Instantiate(string path, Vector3 position)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        
        if (original == null)
        {
            Debug.Log($"Failed to load Prefab : {path}");
            return null;
        }

        if (original.GetComponent<Poolable>() != null)
            return Managers.Pool.Pop(original).gameObject;

        GameObject go = Object.Instantiate(original, position, Quaternion.identity);
        go.name = original.name;
        return go;
    }

    public GameObject InstantiateFromContainer(string path, Transform parent = null)
    {
        var original = Load<GameObject>($"Prefabs/{path}");
        if (original == null)
        {
            Debug.Log($"Failed to load Prefab : {path}");
            return null;
        }

        var installer = new ServiceInstaller();

        // Register services that are needed for the entire project
        var projectContext = ProjectContext.Instance.Container;
        installer.CreateFactoryOnProjectContext(projectContext);
        
        // Register services that are needed for the scene (especially view models)
        var sceneContext = Object.FindAnyObjectByType<SceneContext>().Container;
        installer.CreateFactory(path, sceneContext);
        
        var instance = sceneContext.InstantiatePrefab(original, parent);
        
        return instance;
    }

    public void Destroy(GameObject go, float time)
    {
        if (go == null)
        {
            return;
        }

        Poolable poolable = go.GetComponent<Poolable>();
        if (poolable != null)
        {
            Managers.Pool.Push(poolable);
            return;
        }
        
        Object.Destroy(go, time);
    }
    
    public void Destroy(GameObject go)
    {
        if (go == null)
        {
            return;
        }

        Poolable poolable = go.GetComponent<Poolable>();
        if (poolable != null)
        {
            Managers.Pool.Push(poolable);
            return;
        }
        
        Object.Destroy(go);
    }
    
    public IEnumerator Despawn(GameObject go, float time)
    {
        yield return new WaitForSeconds(time * Time.deltaTime);
        Destroy(go);
    }

    IEnumerator DestroyAndPush(GameObject go, float time)
    {
        yield return new WaitForSeconds(time * Time.deltaTime);
        
        Poolable poolable = go.GetComponent<Poolable>();
        if (poolable != null)
        {
            Managers.Pool.Push(poolable);
        }
        
        Object.Destroy(go, time * Time.deltaTime);
    }
    
    public GameObject GetCardResources<TEnum>(
        IAsset asset, Transform parent, Action<PointerEventData> action = null) where TEnum : struct, Enum
    {
        var cardFrame = Instantiate("UI/Deck/CardFrame", parent);
        var unitInCard = Util.FindChild(cardFrame, "CardUnit", true);
        var card = cardFrame.GetOrAddComponent<Card>();
        
        card.Id = asset.Id;
        card.Class = asset.Class;
        card.AssetType = typeof(TEnum).Name switch
        {
            "UnitId" => Asset.Unit,
            "SheepId" => Asset.Sheep,
            "EnchantId" => Asset.Enchant,
            "CharacterId" => Asset.Character,
            _ => Asset.None
        };
        
        var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), asset.Id);
        var path = $"Sprites/Portrait/{enumValue.ToString()}";
        var background = Util.FindChild(cardFrame, "Bg", true).GetComponent<Image>();
        var gradient = Util.FindChild(cardFrame, "Gradient", true).GetComponent<Image>();
        var glow = Util.FindChild(cardFrame, "Glow", true).GetComponent<Image>();
        var role = Util.FindChild(cardFrame, "Role", true);
        var roleIcon = Util.FindChild(role, "RoleIcon", true).GetComponent<Image>();
        
        unitInCard.GetComponent<Image>().sprite = Load<Sprite>(path);
        BindUnitCardColor(asset.Class, background, gradient, glow);
        BindUnitRoleIcon(card.Id, roleIcon);
        
        if (action != null) cardFrame.BindEvent(action);
        if (card.AssetType != Asset.Unit)
        {
            role.SetActive(false);
            return cardFrame;
        }
        if (Managers.Data.UnitInfoDict.TryGetValue(asset.Id, out var unitInfo) == false) return cardFrame;
        var starPanel = cardFrame.transform.Find("StarPanel");
        var nameText = Util.FindChild(cardFrame, "UnitNameText", true).GetComponent<TextMeshProUGUI>();
        nameText.text = ((UnitId)unitInfo.Id).ToString();
            
        for (var i = 0; i < 3; i++)
        {
            starPanel.GetChild(i).gameObject.SetActive(i < unitInfo.Level);
        }
        
        return cardFrame;
    }
    
    public GameObject GetMaterialResources(IAsset asset, Transform parent, Action<PointerEventData> action = null)
    {
        var itemFrame = Instantiate("UI/Deck/ItemFrame", parent);
        var materialInFrame = Util.FindChild(itemFrame, "ItemImage", true, true);
        var material = itemFrame.GetOrAddComponent<MaterialItem>();
        
        material.Id = asset.Id;
        material.Class = asset.Class;
        
        var enumValue = (MaterialId)Enum.ToObject(typeof(MaterialId), asset.Id);
        var path = $"Sprites/Materials/{enumValue.ToString()}";
        var background = Util.FindChild(itemFrame, "Bg", true).GetComponent<Image>();
        var cornerDeco = Util.FindChild(itemFrame, "CornerDeco", true).GetComponent<Image>();
        var light = Util.FindChild(itemFrame, "Light", true).GetComponent<Image>();
        var glow = Util.FindChild(itemFrame, "Glow", true).GetComponent<Image>();
        
        materialInFrame.GetComponent<Image>().sprite = Load<Sprite>(path);
        BindMaterialCardColor(material.Class, background, cornerDeco, light, glow);
        
        if (action != null) itemFrame.BindEvent(action);
        
        return itemFrame;
    }
    
    private void BindUnitCardColor(UnitClass unitClass, Image background, Image gradient, Image glow)
    {
        switch (unitClass)
        {
            case UnitClass.Squire:
                background.color = new Color(52 / 255f, 177 / 255f, 83 / 255f);
                gradient.color = new Color(90 / 255f, 216 / 255f, 72 / 255f);
                glow.color = new Color(156 / 255f, 254 / 255f, 79 / 255f);
                break;
            case UnitClass.Knight:
                background.color = new Color(60 / 255f, 136 / 255f, 246 / 255f);
                gradient.color = new Color(6 / 255f, 172 / 255f, 254 / 255f);
                glow.color = new Color(1 / 255f, 222 / 255f, 1);
                break;
            case UnitClass.NobleKnight:
                background.color = new Color(115 / 255f, 77 / 255f, 238 / 255f);
                gradient.color = new Color(149 / 255f, 85 / 255f, 253 / 255f);
                glow.color = new Color(185 / 255f, 150 / 255f, 1);
                break;
            case UnitClass.Baron:
                background.color = new Color(1, 201 / 255f, 0);
                gradient.color = new Color(1, 245 / 255f, 34 / 255f);
                glow.color = new Color(1, 245 / 255f, 200 / 255f);
                break;
            case UnitClass.Peasant:
            case UnitClass.None:
            default:
                background.color = new Color(98 / 255f, 110 / 255f, 139 / 255f);
                gradient.color = new Color(144 / 255f, 163 / 255f, 186 / 255f);
                glow.color = new Color(131 / 255f, 166 / 255f, 180 / 255f);
                break;
        }
    }
    
    private void BindUnitRoleIcon(int id, Image roleIcon)
    {
        Managers.Data.UnitInfoDict.TryGetValue(id, out var unitInfo);
        if (unitInfo == null) return;
        
        var path = $"Sprites/Icons/icon_role_{unitInfo.Role.ToString().ToLower()}";
        roleIcon.sprite = Load<Sprite>(path);
    }
    
    private void BindMaterialCardColor(UnitClass materialClass, Image background, Image deco, Image light, Image glow)
    {
        switch (materialClass)
        {
            case UnitClass.Squire:
                background.color = new Color(29 / 255f, 192 / 255f, 86 / 255f);
                deco.color = new Color(52 / 255f, 217 / 255f, 52 / 255f);
                light.color = new Color(178 / 255f, 241 / 255f, 31 / 255f);
                glow.color = new Color(192 / 255f, 255 / 255f, 81 / 255f);
                break;
            case UnitClass.Knight:
                background.color = new Color(0 / 255f, 168 / 255f, 255 / 255f);
                deco.color = new Color(44 / 255f, 190 / 255f, 255 / 255f);
                light.color = new Color(53 / 255f, 251 / 255f, 255 / 255f);
                glow.color = new Color(8 / 255f, 239 / 255f, 255 / 255f);
                break;
            case UnitClass.NobleKnight:
                background.color = new Color(178 / 255f, 96 / 255f, 253 / 255f);
                deco.color = new Color(200 / 255f, 116 / 255f, 253 / 255f);
                light.color = new Color(1f, 138 / 255f, 1f);
                glow.color = new Color(185 / 255f, 138 / 255f, 1f);
                break;
            case UnitClass.Baron:
                background.color = new Color(1f, 201 / 255f, 0);
                deco.color = new Color(1f, 222 / 255f, 0);
                light.color = new Color(254 / 255f, 138 / 255f, 78 / 255f);
                glow.color = new Color(243 / 244f, 1f, 49 / 255f);
                break;
            case UnitClass.Peasant:
            case UnitClass.None:
            default:
                background.color = new Color(97 / 255f, 126 / 255f, 138 / 255f);
                deco.color = new Color(113 / 255f, 142 / 255f, 153 / 255f);
                light.color = new Color(130 / 255f, 160 / 255f, 171 / 255f);
                glow.color = new Color(131 / 255f, 166 / 255f, 180 / 255f);
                break;
        }
    }
    
    public Image GetFrameFromCardButton(ISkillButton button)
    {
        if (button is not MonoBehaviour mono) return null;
        var go = mono.gameObject;
        return go.transform.parent.parent.GetChild(1).GetComponent<Image>();
    }

    public GameObject GetFriendFrame(FriendUserInfo friendInfo, Transform parent, Action<PointerEventData> action = null)
    {
        var frame = Instantiate("UI/Deck/FriendInfo", parent);
        var nameText = Util.FindChild(frame, "NameText", true).GetComponent<TextMeshProUGUI>();
        var rankText = Util.FindChild(frame, "RankText", true).GetComponent<TextMeshProUGUI>();
        var actText = Util.FindChild(frame, "UserActText", true).GetComponent<TextMeshProUGUI>();
        
        nameText.text = friendInfo.UserName;
        rankText.text = friendInfo.RankPoint.ToString();
        actText.text = friendInfo.Act.ToString();
        frame.GetOrAddComponent<Friend>().FriendName = friendInfo.UserName;
        frame.BindEvent(action);
        
        return frame;
    }

    public GameObject GetFriendInviteFrame(
        FriendUserInfo friendInfo,
        Transform parent,
        Action<PointerEventData> action = null)
    {
        var frame = Instantiate("UI/Deck/FriendInviteFrame", parent);
        var nameText = Util.FindChild(frame, "NameText", true).GetComponent<TextMeshProUGUI>();
        var rankText = Util.FindChild(frame, "RankText", true).GetComponent<TextMeshProUGUI>();
        var actText = Util.FindChild(frame, "UserActText", true).GetComponent<TextMeshProUGUI>();
        
        nameText.text = friendInfo.UserName;
        rankText.text = friendInfo.RankPoint.ToString();
        actText.text = friendInfo.Act.ToString();
        frame.GetOrAddComponent<Friend>().FriendName = friendInfo.UserName;
        frame.BindEvent(action);
        
        return frame;
    }
    
    public GameObject GetFriendRequestFrame(
        FriendUserInfo friendInfo, 
        Transform parent, 
        Action<PointerEventData> action = null)
    {
        var frame = Instantiate("UI/Deck/FriendRequestInfo", parent);
        var nameText = Util.FindChild(frame, "NameText", true).GetComponent<TextMeshProUGUI>();
        var rankText = Util.FindChild(frame, "RankText", true).GetComponent<TextMeshProUGUI>();
        
        nameText.text = friendInfo.UserName;
        rankText.text = friendInfo.RankPoint.ToString();
        frame.GetOrAddComponent<Friend>().FriendName = friendInfo.UserName;
        frame.BindEvent(action);
        
        return frame;
    }

    public GameObject GetProductMailFrame(MailInfo mailInfo, Transform parent, Action<PointerEventData> action = null)
    {
        var frame = Instantiate("UI/Deck/MailInfoProduct", parent);
        var claimButton = Util.FindChild(frame, "ClaimButton", true);
        var countText = Util.FindChild(frame, "CountText", true).GetComponent<TextMeshProUGUI>();
        var infoText = Util.FindChild(frame, "InfoText", true).GetComponent<TextMeshProUGUI>();
        var expiresText = Util.FindChild(frame, "ExpiresText", true).GetComponent<TextMeshProUGUI>();  
        
        frame.GetComponent<Mail>().MailId = mailInfo.MailId;
        countText.gameObject.SetActive(false);
        infoText.text = mailInfo.Message;
        expiresText.text = mailInfo.ExpiresAt.ToString(CultureInfo.CurrentCulture);
        claimButton.BindEvent(action);

        return frame;
    }

    public GameObject GetInviteMailFrame(MailInfo mailInfo, Transform parent)
    {
        var frame = Instantiate("UI/Deck/MailInfoInvitation", parent);
        var infoText = Util.FindChild(frame, "InfoText", true).GetComponent<TextMeshProUGUI>();
        var expireText = Util.FindChild(frame, "ExpireText", true).GetComponent<TextMeshProUGUI>();
        
        frame.GetComponent<Mail>().MailId = mailInfo.MailId;
        infoText.text = mailInfo.Message;
        expireText.text = mailInfo.ExpiresAt.ToString(CultureInfo.CurrentCulture);
        
        return frame;
    }
    
    public GameObject GetNotifyMailFrame(MailInfo mailInfo, Transform parent, Action<PointerEventData> action = null)
    {
        var frame = Instantiate("UI/Deck/MailInfoNotification", parent);
        var claimButton = Util.FindChild(frame, "ClaimButton", true);
        var infoText = Util.FindChild(frame, "InfoText", true).GetComponent<TextMeshProUGUI>();
        
        frame.GetComponent<Mail>().MailId = mailInfo.MailId;
        infoText.text = mailInfo.Message;
        claimButton.BindEvent(action);
        
        return frame;
    }
}