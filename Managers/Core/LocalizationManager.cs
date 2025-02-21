using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;

public class LocalizationManager
{
    private TextMeshProUGUI _uiText;
    private string _language2Letter;

    private Dictionary<string, Dictionary<string, Contents.LocalizationEntry>> _localizationDict = new();
    private readonly Dictionary<string, string> _languageMap = new()
    {
        {"Korean", "ko"},
        {"English", "en"},
        {"Japanese", "ja"},
        {"Chinese", "zh"},
        {"Russian", "ru"},
        {"Spanish", "es"},
        {"German", "de"},
        {"French", "fr"},
        {"Italian", "it"},
        {"Dutch", "nl"},
        {"Turkish", "tr"},
        {"Portuguese", "pt"},
        {"Arabic", "ar"},
        {"Vietnamese", "vi"},
        {"Thai", "th"},
        {"Indonesian", "id"},
        {"Malay", "ms"},
        {"Hindi", "hi"},
        {"Bengali", "bn"},
        {"Tagalog", "tl"},
        {"Polish", "pl"},
        {"Ukrainian", "uk"},
        {"Czech", "cs"},
        {"Greek", "el"},
        {"Swedish", "sv"},
    };

    private readonly Dictionary<string, string> _basicFontMap = new()
    {
        { "ko", "esamanru Medium SDF" },
        { "en", "Sen SDF" },
    };
    
    public void Init()
    {
        _languageMap.TryGetValue(GetLanguage(), out _language2Letter);
    }
    
    private string GetLanguage()
    {
        return PlayerPrefs.HasKey("Language") 
            ? PlayerPrefs.GetString("Language") 
            : Application.systemLanguage.ToString();
    }
    
    private TMP_FontAsset GetFont(string fontName)
    {
        return Resources.Load<TMP_FontAsset>($"Fonts/{fontName}");
    }

    public string GetConvertedString(string str)
    {
        return Regex.Replace(str, "([a-z])([A-Z])", "$1_$2").ToLower();
    }
    
    public void UpdateTextAndFont(Dictionary<string, GameObject> textDict)
    {
        foreach (var go in textDict.Values)
        {
            // ex) "ContinueButtonText" -> "continue_button_text"
            UpdateTextAndFont(go, GetConvertedString(go.name));
        }
    }

    public void UpdateTextAndFont(GameObject go, string key, string additionalText = "")
    {
        if (_localizationDict == null || _localizationDict.Count == 0)
        {
            _localizationDict = Managers.Data.LocalizationDict;
        }
        
        // entryDictionary = "continue_button_text": { "en": { ... }, "ko": { ... } }
        if (_localizationDict.TryGetValue(key, out var entryDictionary) == false) return;
        // entry = "en": { "text": "Continue", "font": Sen SDF", "fontSize": 20 }
        if (entryDictionary.TryGetValue(_language2Letter, out var entry) == false) return;
        
        if (go.TryGetComponent(out TextMeshProUGUI tmpro))
        {
            tmpro.text = entry.Text + additionalText;
            tmpro.font = GetFont(entry.Font);
            if (entry.FontSize != 0)
            {
                tmpro.fontSize = entry.FontSize;
            }
        }
        else
        {
            Debug.LogError($"Failed to get TextMeshProUGUI component: {go.name}");
        }
    }

    public string GetLocalizedValue(TextMeshProUGUI tmpro, string key)
    {
        if (_localizationDict == null || _localizationDict.Count == 0)
        {
            _localizationDict = Managers.Data.LocalizationDict;
        }
        
        if (_localizationDict.TryGetValue(key, out var entryDictionary) == false) return string.Empty;
        if (entryDictionary.TryGetValue(_language2Letter, out var entry) == false) return string.Empty;
        tmpro.font = GetFont(entry.Font);
        if (entry.FontSize != 0)
        {
            tmpro.fontSize = entry.FontSize;
        }
        
        return entry.Text;
    }

    public void GetLocalizedSkillText(TextMeshProUGUI tmpro, Contents.SkillData skill, int unitId)
    {
        if (_localizationDict == null || _localizationDict.Count == 0)
        {
            _localizationDict = Managers.Data.LocalizationDict;
        }
        
        var skillKey = $"skill_id_text_{skill.Id}";
        var unitName = ((UnitId)unitId).ToString();
        var unitRegex = Regex.Replace(unitName, "([a-z])([A-Z])", "$1_$2").ToLower();
        var unitKey = $"unit_name_{unitRegex}";
        
        if (_localizationDict.TryGetValue(skillKey, out var entryDictionary) == false) return;
        if (_localizationDict.TryGetValue(unitKey, out var unitEntryDictionary) == false) return;
        if (entryDictionary.TryGetValue(_language2Letter, out var entry) == false) return;
        if (unitEntryDictionary.TryGetValue(_language2Letter, out var unitEntry) == false) return;

        var formattedUnitName = unitEntry.Text;
        var placeholders = GetPlaceholders(entry.Text);
        var replaceDict = new Dictionary<string, object>();
        var unitSkill = (int)(Managers.Data.UnitDict[unitId].Stat.Skill * skill.Coefficient);
        
        switch (placeholders.Count)
        {
            case 1:
                replaceDict.Add("name", formattedUnitName);
                break;
            
            case 2:
                if (placeholders.ContainsKey("skill"))
                {
                    replaceDict.Add("name", formattedUnitName);
                    replaceDict.Add("skill", unitSkill);
                }
                else
                {
                    replaceDict.Add("name", formattedUnitName);
                    replaceDict.Add("value", skill.Value);
                }
                break;
            
            case 3:
                replaceDict.Add("name", formattedUnitName);
                replaceDict.Add("skill", unitSkill);
                replaceDict.Add("value", skill.Value);
                break;
        }
        
        tmpro.text = FormatSkillText(entry.Text, replaceDict);
        tmpro.font = GetFont(entry.Font);
        
        if (entry.FontSize != 0)
        {
            tmpro.fontSize = entry.FontSize;
        }
    }
    
    public void UpdateNotifyPopupText(UI_NotifyPopup popup, string titleKey, string messageKey)
    {
        if (_localizationDict == null || _localizationDict.Count == 0)
        {
            _localizationDict = Managers.Data.LocalizationDict;
        }
        
        if (_localizationDict[titleKey].TryGetValue(_language2Letter, out var titleEntry) &&
            _localizationDict[messageKey].TryGetValue(_language2Letter, out var messageEntry) &&
            _localizationDict["confirm_button_text"].TryGetValue(_language2Letter, out var buttonEntry))
        {
            popup.TitleText = titleEntry.Text;
            popup.TitleFont = GetFont(titleEntry.Font);
            popup.TitleFontSize = titleEntry.FontSize;
            popup.MessageText = messageEntry.Text;
            popup.MessageFont = GetFont(messageEntry.Font);
            popup.MessageFontSize = messageEntry.FontSize;
            popup.ButtonText = buttonEntry.Text;
            popup.ButtonFont = GetFont(buttonEntry.Font);
            popup.ButtonFontSize = buttonEntry.FontSize;
        }
        else
        {
            Debug.LogError($"Translation key not found: {titleKey} or {messageKey}");
        }
    }

    public void UpdateNotifySelectPopupText(UI_NotifySelectPopup popup, string titleKey, string messageKey)
    {
        if (_localizationDict == null || _localizationDict.Count == 0)
        {
            _localizationDict = Managers.Data.LocalizationDict;
        }
        
        if (_localizationDict[titleKey].TryGetValue(_language2Letter, out var titleEntry) &&
            _localizationDict[messageKey].TryGetValue(_language2Letter, out var messageEntry))
        {
            popup.TitleText = titleEntry.Text;
            popup.TitleFont = GetFont(titleEntry.Font);
            popup.TitleFontSize = titleEntry.FontSize;
            popup.MessageText = messageEntry.Text;
            popup.MessageFont = GetFont(messageEntry.Font);
            popup.MessageFontSize = messageEntry.FontSize;
        }
        else
        {
            Debug.LogError($"Translation key not found: {titleKey} or {messageKey}");
        }
    }

    public void UpdateInputFieldFont(TMP_InputField inputField, string fontName = null)
    {
        if (fontName == null)
        {
            _basicFontMap.TryGetValue(_language2Letter, out fontName);
        }
        
        inputField.fontAsset = GetFont(fontName);
    }
    
    public void UpdateTextFont(GameObject go, string fontName = null)
    {
        if (fontName == null)
        {
            _basicFontMap.TryGetValue(_language2Letter, out fontName);
        }
        
        if (go.TryGetComponent(out TextMeshProUGUI text))
        {
            text.font = GetFont(fontName);
        }
    }

    /// <summary>
    /// 텍스트 내의 모든 명명된 플레이스홀더를 찾아 개수와 이름을 반환
    /// </summary>
    /// <param name="text">플레이스홀더가 포함된 텍스트</param>
    /// <returns>플레이스홀더 이름을 키로 하고, 해당 플레이스홀더가 등장한 횟수를 값으로 하는 딕셔너리</returns>
    private Dictionary<string, int> GetPlaceholders(string text)
    {
        var placeholders = new Dictionary<string, int>();
        if (string.IsNullOrEmpty(text)) return placeholders;
        
        var regex = new Regex(@"\{(\w+)\}");
        var matches = regex.Matches(text);
        
        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value;
            if (placeholders.TryAdd(key, 1) == false)
            {
                placeholders[key]++;
            }
        }

        return placeholders;
    }

    
    /// <summary>
    /// 텍스트 내의 플레이스홀더를 대체 값으로 변경
    /// </summary>
    /// <param name="text">포맷팅할 원본 텍스트 (플레이스홀더 포함)</param>
    /// <param name="placeholders">플레이스홀더와 그 대체 값이 담긴 딕셔너리</param>
    /// <returns>플레이스홀더가 대체된 포맷팅된 텍스트</returns>
    private string FormatSkillText(string text, Dictionary<string, object> placeholders)
    {
        if (string.IsNullOrEmpty(text) || placeholders == null || placeholders.Count == 0) return text;

        foreach (var pair in placeholders)
        {
            var placeholder = "{" + pair.Key + "}";
            var value = pair.Value?.ToString() ?? string.Empty;
            text = text.Replace(placeholder, value);
        }

        return text;
    }
}
