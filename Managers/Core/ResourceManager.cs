using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening.Plugins.Core.PathCore;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

public class ResourceManager
{
    private const string MovedRoot = "Assets/Resources_moved/";
    
    public bool InitAddressables { get; set; }

    private bool ExistsInAddressables(string key)
    {
        return Addressables.ResourceLocators.Any(loc => loc.Keys.Contains(key));
    }

    /// <summary>
    /// Asynchronously Loading according to Addressables -> Pool -> Resources.
    /// If Fast-Follow / ODR bundles are needed, pending here.
    /// </summary>
    public async Task<T> LoadAsync<T>(string key, string extension = "png") where T : Object
    {
        // Check pool first
        if (typeof(T) == typeof(GameObject))
        {
            string name = Util.ExtractName(key);
            GameObject pooledObject = Managers.Pool.GetOriginal(name);
            if (pooledObject != null) 
                return pooledObject as T;
        }

        if (Managers.Network.UseAddressables)
        {
            // Addressables
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>($"{key}.{extension}");
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }

            Debug.LogError($"Failed to load asset from Addressables: {key}.{extension}");
            Addressables.Release(handle); 
        }
        else
        {
#if UNITY_EDITOR
            // Editor -> Resources_Moved
            string editorPath = FindInMovedFolder<T>(key);
            if (editorPath != null)
                return AssetDatabase.LoadAssetAtPath<T>(editorPath);
#endif
        }

        Debug.Log("Not exists in Addressables : " + key);
        return null;
    }
    
#if UNITY_EDITOR
    /* ───── Editor 전용: .prefab / .png / .asset 등 확장자 자동 추적 ───── */
    private static string FindInMovedFolder<T>(string key) where T : Object
    {
        // "Prefabs/UI/Menu/StartButton" → dir="Prefabs/UI/Menu", name="StartButton"
        string cleaned = key.TrimStart('/');
        string wantedDir = System.IO.Path.GetDirectoryName(cleaned)?.Replace('\\', '/');      
        string wantedName = System.IO.Path.GetFileNameWithoutExtension(cleaned);            
        
        // Searching directory
        string absoluteDir = string.IsNullOrEmpty(wantedDir) ? MovedRoot.TrimEnd('/') : $"{MovedRoot}{wantedDir}";
        if (Directory.Exists(absoluteDir))
        {
            string match = Directory
                .GetFiles(absoluteDir, $"{wantedName}.*", SearchOption.TopDirectoryOnly)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(match) == false) return match.Replace('\\', '/');
        }

        // Searching all guids
        string[] guids = AssetDatabase.FindAssets(wantedName, new[] { MovedRoot });
        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string relativeDir = System.IO.Path.GetDirectoryName(assetPath)?
                .Replace(MovedRoot, string.Empty)
                .Replace('\\', '/');
            
            if (System.IO.Path.GetFileNameWithoutExtension(assetPath).Equals(wantedName, StringComparison.OrdinalIgnoreCase) &&
                (relativeDir ?? string.Empty).Equals(wantedDir ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return assetPath;
            }
        }

        return null;
    }
#endif
    
    public async Task<GameObject> Instantiate(string key, Transform parent = null)
    {
        GameObject original = await LoadAsync<GameObject>($"Prefabs/{key}", "prefab");
        
        if (original == null)
        {
            Debug.Log($"Failed to load Prefab : {key}");
            return null;
        }

        if (original.TryGetComponent(out Poolable _))
        {
            return Managers.Pool.Pop(original, parent).gameObject;
        }

        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        return go;
    }   
    
    public async Task<GameObject> Instantiate(string key, Vector3 position)
    {
        GameObject original = await LoadAsync<GameObject>($"Prefabs/{key}", "prefab");
        
        if (original == null)
        {
            Debug.Log($"Failed to load Prefab : {key}");
            return null;
        }

        if (original.TryGetComponent(out Poolable _))
        {
            return Managers.Pool.Pop(original).gameObject;
        }

        GameObject go = Object.Instantiate(original, position, Quaternion.identity);
        go.name = original.name;
        return go;
    }

    public async Task<GameObject> InstantiateAsyncFromContainer(string key, Transform parent = null)
    {
        GameObject original = await LoadAsync<GameObject>($"Prefabs/{key}", "prefab");
        if (original == null)
        {
            Debug.Log($"Failed to load Prefab : {key}");
            return null;
        }
        
        var installer = new ServiceInstaller();

        // Register services that are needed for the entire project
        var projectContext = ProjectContext.Instance.Container;
        installer.CreateFactoryOnProjectContext(projectContext);
        
        // Register services that are needed for the scene (especially view models)
        var sceneContext = Object.FindAnyObjectByType<SceneContext>().Container;
        installer.CreateFactory(key, sceneContext);
        
        return sceneContext.InstantiatePrefab(original, parent);    
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
    
    public async Task<GameObject> GetCardResourcesF<TEnum>(
        IAsset asset, 
        Transform parent, 
        Func<PointerEventData, Task> action = null, 
        bool activateText = false) where TEnum : struct, Enum
    {
        var cardFrame = await Instantiate("UI/Deck/CardFrame", parent);
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
        
        if (action != null) cardFrame.BindEvent(action);
        await SetCardContents<TEnum>(cardFrame, asset, card.AssetType);
        
        var nameTextObject = Util.FindChild(cardFrame, "UnitNameText", true, true);
        nameTextObject.SetActive(activateText);
        
        return cardFrame;
    }
    
    public async Task<GameObject> GetCardResources<TEnum>(
        IAsset asset, 
        Transform parent, 
        Action<PointerEventData> action = null, 
        bool activateText = false) where TEnum : struct, Enum
    {
        var cardFrame = await Instantiate("UI/Deck/CardFrame", parent);
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
        
        if (action != null) cardFrame.BindEvent(action);
        await SetCardContents<TEnum>(cardFrame, asset, card.AssetType);
        
        var nameTextObject = Util.FindChild(cardFrame, "UnitNameText", true, true);
        nameTextObject.SetActive(activateText);
        
        return cardFrame;
    }

    private async Task SetCardContents<TEnum>(GameObject cardFrame, IAsset asset, Asset assetType) where TEnum : struct, Enum
    {
        var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), asset.Id);
        var portraitKey = $"Sprites/Portrait/{enumValue.ToString()}";
        var background = Util.FindChild(cardFrame, "Bg", true).GetComponent<Image>();
        var gradient = Util.FindChild(cardFrame, "Gradient", true).GetComponent<Image>();
        var glow = Util.FindChild(cardFrame, "Glow", true).GetComponent<Image>();
        var role = Util.FindChild(cardFrame, "Role", true);
        var roleIcon = Util.FindChild(role, "RoleIcon", true).GetComponent<Image>();
        var unitInCard = Util.FindChild(cardFrame, "CardUnit", true);
        var startPanel = Util.FindChild(cardFrame, "StarPanel", true);
        var nameTextObject = Util.FindChild(cardFrame, "UnitNameText", true, true);
        
        unitInCard.GetComponent<Image>().sprite = await LoadAsync<Sprite>(portraitKey);
        BindUnitCardColor(asset.Class, background, gradient, glow);
        await BindUnitRoleIcon(asset.Id, roleIcon);

        var key = string.Empty;
        switch (assetType)
        {
            case Asset.Unit:
                if (Managers.Data.UnitInfoDict.TryGetValue(asset.Id, out var unitInfo))
                {
                    var unitName = ((UnitId)unitInfo.Id).ToString();
                    key = string.Concat("unit_name_", Managers.Localization.GetConvertedString(unitName));
                    for (var i = 0; i < 3; i++)
                    {
                        startPanel.transform.GetChild(i).gameObject.SetActive(i < unitInfo.Level);
                    }
                }
                break;
            
            case Asset.Character:
                role.SetActive(false);
                startPanel.gameObject.SetActive(false);
                if (Managers.Data.CharacterInfoDict.TryGetValue(asset.Id, out var characterInfo))
                {
                    var characterName = ((CharacterId)characterInfo.Id).ToString();
                    key = string.Concat("character_name_", Managers.Localization.GetConvertedString(characterName));
                }
                break;
            
            case Asset.Sheep:
                role.SetActive(false);
                startPanel.gameObject.SetActive(false);
                if (Managers.Data.SheepInfoDict.TryGetValue(asset.Id, out var sheepInfo))
                {
                    var sheepName = ((SheepId)sheepInfo.Id).ToString();
                    key = string.Concat("sheep_name_", Managers.Localization.GetConvertedString(sheepName));
                }
                break;
            
            case Asset.Enchant:
                var enchantRect = unitInCard.GetComponent<RectTransform>();
                enchantRect.anchorMin = Vector2.zero;
                enchantRect.anchorMax = Vector2.one;
                enchantRect.sizeDelta = Vector2.zero;
                role.SetActive(false);
                startPanel.gameObject.SetActive(false);
                if (Managers.Data.EnchantInfoDict.TryGetValue(asset.Id, out var enchantInfo))
                {
                    var enchantName = ((EnchantId)enchantInfo.Id).ToString();
                    key = string.Concat("enchant_name_", Managers.Localization.GetConvertedString(enchantName));
                }
                break;
        }

        if (key != string.Empty)
        {
            var convertedKey = Managers.Localization.GetConvertedString(key);
            await Managers.Localization.UpdateTextAndFont(nameTextObject, convertedKey);
        }
    }
    
    public async Task<GameObject> GetMaterialResources(IAsset asset, Transform parent, Action<PointerEventData> action = null)
    {
        var itemFrame = await Instantiate("UI/Deck/ItemFrame", parent);
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
        
        materialInFrame.GetComponent<Image>().sprite = await LoadAsync<Sprite>(path);
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
    
    private async Task BindUnitRoleIcon(int id, Image roleIcon)
    {
        Managers.Data.UnitInfoDict.TryGetValue(id, out var unitInfo);
        if (unitInfo == null) return;
        
        var path = $"Sprites/Icons/icon_role_{unitInfo.Role.ToString().ToLower()}";
        roleIcon.sprite = await LoadAsync<Sprite>(path);
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

    public async Task<GameObject> GetFriendFrame(FriendUserInfo friendInfo, Transform parent, Action<PointerEventData> action = null)
    {
        var frame = await Instantiate("UI/Deck/FriendInfo", parent);
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

    public async Task<GameObject> GetFriendInviteFrame(
        FriendUserInfo friendInfo,
        Transform parent,
        Action<PointerEventData> action = null)
    {
        var frame = await Instantiate("UI/Deck/FriendInviteFrame", parent);
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
    
    public async Task<GameObject> GetFriendRequestFrame(
        FriendUserInfo friendInfo, 
        Transform parent, 
        Action<PointerEventData> action = null)
    {
        var frame = await Instantiate("UI/Deck/FriendRequestInfo", parent);
        var nameText = Util.FindChild(frame, "NameText", true).GetComponent<TextMeshProUGUI>();
        var rankText = Util.FindChild(frame, "RankText", true).GetComponent<TextMeshProUGUI>();
        
        nameText.text = friendInfo.UserName;
        rankText.text = friendInfo.RankPoint.ToString();
        frame.GetOrAddComponent<Friend>().FriendName = friendInfo.UserName;
        frame.BindEvent(action);
        
        return frame;
    }

    public async Task<GameObject> GetProductMailFrame(MailInfo mailInfo, Transform parent)
    {
        var frame = await Instantiate("UI/Deck/MailInfoProduct", parent);
        var claimButton = Util.FindChild(frame, "ClaimButton", true);
        var countText = Util.FindChild(frame, "CountText", true).GetComponent<TextMeshProUGUI>();
        var infoText = Util.FindChild(frame, "InfoText", true).GetComponent<TextMeshProUGUI>();
        var expiresText = Util.FindChild(frame, "ExpiresText", true).GetComponent<TextMeshProUGUI>();  
        
        frame.GetOrAddComponent<MailInfoProduct>().MailInfo = mailInfo;
        countText.gameObject.SetActive(false);
        infoText.text = mailInfo.Message;
        expiresText.text = mailInfo.ExpiresAt.ToString(CultureInfo.CurrentCulture);

        return frame;
    }
    
    public RenderTexture CreateRenderTexture(string textureName)
    {  
        var rt = new RenderTexture(256, 256, 0)
        {
            useMipMap = false,
            autoGenerateMips = false,
            antiAliasing = 1,
            name = textureName,
            graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB,
            depthStencilFormat = GraphicsFormat.D16_UNorm,
        };
        
        rt.Create();
        
        return rt;
    }
}