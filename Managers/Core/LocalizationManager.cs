using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;

public class LocalizationManager
{
    private TextMeshProUGUI _uiText;
    private string _language2Letter;

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

    private readonly Dictionary<string, string> _boldFontMap = new()
    {
        { "ko", "esamanru Bold SDF" },
        { "en", "Sen SDF" },
        { "ja", "mplus-1p-bold SDF" }
    };

    private readonly Dictionary<string, string> _basicFontMap = new()
    {
        { "ko", "esamanru Medium SDF" },
        { "en", "Sen SDF" },
        { "ja", "mplus-1p-medium SDF"}
    };

    private readonly Dictionary<string, string> _blackLinedFontMap = new()
    {
        { "ko", "esamanru_outline Medium SDF" },
        { "en", "Sen_Line_s_Black SDF" },
        { "ja", "mplus-1p-medium-outline SDF" }
    };

    private readonly Dictionary<string, string> _blueLinedFontMap = new()
    {
        { "ko", "esamanru_outline_blue Medium SDF" },
        { "en", "Sen_Line_s_Blue SDF" },
        { "ja", "mplus-1p-medium-outline-blue SDF" }
    };
    
    private readonly Dictionary<string, string> _redLinedFontMap = new()
    {
        { "ko", "esamanru_outline_red Medium SDF" },
        { "en", "Sen_Line_s_Red SDF" },
        { "ja", "mplus-1p-medium-outline-red SDF" }
    };
    
    public string Language2Letter
    {
        get
        {
            if (string.IsNullOrEmpty(_language2Letter))
            {
                _language2Letter = GetSavedOrSystemLanguage2Letter();
            }
            return _language2Letter;
        }
        
        private set => _language2Letter = NormalizeTo2Letter(value);
    }
    
    public void InitLanguage(string systemLanguageString)
    {
        if (PlayerPrefs.HasKey("Language"))
        {
            SetLanguage(PlayerPrefs.GetString("Language"));
            return;
        }

        SetLanguage(systemLanguageString);
    }
    
    public void SetLanguage(string input)
    {
        var lang2 = NormalizeTo2Letter(input);
        PlayerPrefs.SetString("Language", lang2);
        PlayerPrefs.Save();
        Language2Letter = lang2;
    }

    private string GetSavedOrSystemLanguage2Letter()
    {
        if (PlayerPrefs.HasKey("Language"))
        {
            return NormalizeTo2Letter(PlayerPrefs.GetString("Language"));
        }
        
        return NormalizeTo2Letter(Application.systemLanguage.ToString());
    }
    
    private string NormalizeTo2Letter(string input)
    {
        if (string.IsNullOrEmpty(input)) return "en";

        // "EN", "Ko" 등 처리
        input = input.Trim();
        if (input.Length == 2) return input.ToLowerInvariant();

        if (_languageMap != null && _languageMap.TryGetValue(input, out var mapped))
        {
            var m = mapped?.Trim() ?? "en";
            return m.Length == 2 ? m.ToLowerInvariant() : NormalizeTo2Letter(m);
        }
        
        return input switch
        {
            "Korean" => "ko",
            "English" => "en",
            "Japanese" => "ja",
            "Vietnamese" => "vi",
            _ => "en"
        };
    }
    
    private async Task<TMP_FontAsset> GetFont(string fontName)
    {
        return await Managers.Resource.LoadAsync<TMP_FontAsset>($"Fonts/{fontName}", "asset");
    }

    private async Task<TMP_FontAsset> GetFontFromDictionary(FontType fontType)
    {
        var fontName = fontType switch
        {
            FontType.Bold => _boldFontMap.GetValueOrDefault(Language2Letter, "esamanru Bold SDF"),
            FontType.BlackLined => _blackLinedFontMap.GetValueOrDefault(Language2Letter, "esamanru_outline Medium SDF"),
            FontType.BlueLined => _blueLinedFontMap.GetValueOrDefault(Language2Letter, "esamanru_outline_blue Medium SDF"),
            FontType.RedLined => _redLinedFontMap.GetValueOrDefault(Language2Letter, "esamanru_outline_red Medium SDF"),
            _ => _basicFontMap.GetValueOrDefault(Language2Letter, "esamanru Medium SDF")
        };

        return await GetFont(fontName);
    }

    public string GetConvertedString(string str)
    {
        // ex) "ContinueButtonText" -> "continue_button_text"
        return Regex.Replace(str, "([a-z])([A-Z])", "$1_$2").ToLower();
    }
    
    public async Task UpdateTextAndFont(Dictionary<string, GameObject> textDict)
    {
        foreach (var go in textDict.Values)
        {
            await UpdateTextAndFont(go, GetConvertedString(go.name));
        }
    }

    public async Task UpdateTextAndFont(GameObject go, string key, string additionalText = "")
    {
        var langDictionary = Managers.Data.LocalizationDict;
        
        // entryDictionary = "continue_button_text": { "en": { ... }, "ko": { ... } }
        if (langDictionary.TryGetValue(key, out var entryDictionary) == false)
        {
            return;
        }
        // entry = "en": { "text": "Continue", "font": Sen SDF", "fontSize": 20 }
        if (entryDictionary.TryGetValue(Language2Letter, out var entry) == false)
        {
            return;
        }
        
        if (go.TryGetComponent(out TextMeshProUGUI tmpro))
        {
            tmpro.text = entry.Text + additionalText;
            tmpro.font = await GetFont(entry.Font);
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

    public async Task UpdateChangedTextAndFont(Dictionary<string, GameObject> textDict, string language2Letter)
    {
        foreach (var go in textDict.Values)
        {
            await UpdateChangedTextAndFont(go, GetConvertedString(go.name), language2Letter);
        }
    }
    
    private async Task UpdateChangedTextAndFont(GameObject go, string key, string language2Letter)
    {
        var langDictionary = Managers.Data.LocalizationDict;
        
        // entryDictionary = "continue_button_text": { "en": { ... }, "ko": { ... } }
        if (langDictionary.TryGetValue(key, out var entryDictionary) == false) return;
        // entry = "en": { "text": "Continue", "font": Sen SDF", "fontSize": 20 }
        if (entryDictionary.TryGetValue(language2Letter, out var entry) == false) return;
        
        if (go.TryGetComponent(out TextMeshProUGUI tmpro))
        {
            tmpro.text = entry.Text;
            tmpro.font = await GetFont(entry.Font);
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

    public async Task<string> BindLocalizedText(TextMeshProUGUI tmpro, string key, FontType fontType = FontType.None)
    {
        var langDictionary = Managers.Data.LocalizationDict;
        key = GetConvertedString(key);
        if (langDictionary.TryGetValue(key, out var entryDictionary) == false) return string.Empty;
        if (entryDictionary.TryGetValue(Language2Letter, out var entry) == false) return string.Empty;
        tmpro.font = fontType == FontType.None ? await GetFont(entry.Font) : await GetFontFromDictionary(fontType);
        tmpro.text = entry.Text;
        
        if (entry.FontSize != 0)
        {
            tmpro.fontSize = entry.FontSize;
        }
        
        return entry.Text;
    }
    
    public async Task<string> GetLocalizedText(string key)
    {
        var langDictionary = Managers.Data.LocalizationDict;
        key = GetConvertedString(key);
        if (langDictionary.TryGetValue(key, out var entryDictionary) == false) return string.Empty;
        if (entryDictionary.TryGetValue(Language2Letter, out var entry) == false) return string.Empty;
        
        return entry.Text;
    }

    public async Task BindLocalizedSkillText(TextMeshProUGUI tmpro, Contents.SkillData skill, int unitId)
    {
        if (Managers.Data.UnitDict.TryGetValue(unitId, out var unitData) == false)
        {
            // Hand over to Base Skill method
            await BindLocalizedBaseSkillText(tmpro, skill.Id);
            return;
        }
        
        var langDictionary = Managers.Data.LocalizationDict;
        var skillKey = $"skill_id_text_{skill.Id}";
        
        if (langDictionary.TryGetValue(skillKey, out var entryDictionary) == false) return;
        if (entryDictionary.TryGetValue(Language2Letter, out var entry) == false) return;
        
        var placeholders = GetPlaceholders(entry.Text);
        var replaceDict = new Dictionary<string, object>();
        var unitSkill = (int)(unitData.Stat.Skill * skill.Coefficient);
        
        switch (placeholders.Count)
        {
            case 1:
                if (placeholders.ContainsKey("skill"))
                {
                    replaceDict.Add("skill", unitSkill);
                }
                else
                {
                    replaceDict.Add("value", skill.Value);
                }
                break;
            
            case 2:
                replaceDict.Add("skill", unitSkill);
                replaceDict.Add("value", skill.Value);
                break;
        }
        
        tmpro.text = FormatSkillText(entry.Text, replaceDict);
        tmpro.font = await GetFont(entry.Font);
        
        if (entry.FontSize != 0)
        {
            tmpro.fontSize = entry.FontSize;
        }
    }

    public async Task BindLocalizedBaseSkillText(TextMeshProUGUI tmpro, int skillId)
    {
        var langDictionary = Managers.Data.LocalizationDict;
        var skillKey = $"skill_id_text_{skillId}";
        
        if (langDictionary.TryGetValue(skillKey, out var entryDictionary) == false) return;
        if (entryDictionary.TryGetValue(Language2Letter, out var entry) == false) return;
        
        tmpro.text = entry.Text;
        tmpro.font = await GetFont(entry.Font);
        
        if (entry.FontSize != 0)
        {
            tmpro.fontSize = entry.FontSize;
        }
    }

    /// <summary>
    /// In Game Warning Popup
    /// </summary>
    public async Task UpdateWarningPopupText(UI_WarningPopup popup, string messageKey)
    {
        var langDictionary = Managers.Data.LocalizationDict;

        if (langDictionary[messageKey].TryGetValue(Language2Letter, out var entry))
        {
            popup.Text = entry.Text;
            popup.Font = await GetFont(entry.Font);
            popup.FontSize = entry.FontSize;
        }
        else
        {
            Debug.LogError($"Translation key not found: {messageKey}");
        }
    }
    
    public async Task UpdateNotifyPopupText(UI_NotifyPopup popup, string messageKey, string titleKey = "empty_text")
    {
        var langDictionary = Managers.Data.LocalizationDict;
        
        if (langDictionary[titleKey].TryGetValue(Language2Letter, out var titleEntry) &&
            langDictionary[messageKey].TryGetValue(Language2Letter, out var messageEntry) &&
            langDictionary["confirm_button_text"].TryGetValue(Language2Letter, out var buttonEntry))
        {
            popup.TitleText = titleEntry.Text;
            popup.TitleFont = await GetFont(titleEntry.Font);
            popup.TitleFontSize = titleEntry.FontSize;
            popup.MessageText = messageEntry.Text;
            popup.MessageFont = await GetFont(messageEntry.Font);
            popup.MessageFontSize = messageEntry.FontSize;
            popup.ButtonText = buttonEntry.Text;
            popup.ButtonFont = await GetFont(buttonEntry.Font);
            popup.ButtonFontSize = buttonEntry.FontSize;
        }
        else
        {
            popup.MessageText = messageKey;
            Debug.LogError($"Translation key not found: {titleKey} or {messageKey}");
        }
    }

    public async Task UpdateNotifySelectPopupText(UI_NotifySelectPopup popup, string messageKey, string titleKey = "empty_text")
    {
        var langDictionary = Managers.Data.LocalizationDict;
        
        if (langDictionary[titleKey].TryGetValue(Language2Letter, out var titleEntry) &&
            langDictionary[messageKey].TryGetValue(Language2Letter, out var messageEntry))
        {
            popup.TitleText = titleEntry.Text;
            popup.TitleFont = await GetFont(titleEntry.Font);
            popup.TitleFontSize = titleEntry.FontSize;
            popup.MessageText = messageEntry.Text;
            popup.MessageFont = await GetFont(messageEntry.Font);
            popup.MessageFontSize = messageEntry.FontSize;
        }
        else
        {
            Debug.LogWarning($"Translation key not found: {titleKey} or {messageKey}");
        }
    }

    public async Task UpdateInputFieldFont(TMP_InputField inputField, string fontName = null)
    {
        if (fontName == null) 
        {
            _basicFontMap.TryGetValue(Language2Letter, out fontName);
        }

        var font = await GetFont(fontName);
        
        inputField.fontAsset = font;
    }
    
    public async Task<TMP_FontAsset> UpdateFont(TextMeshProUGUI text, FontType fontType = FontType.None, string fontName = null)
    {
        var font = fontName == null ? await GetFontFromDictionary(fontType) : await GetFont(fontName);
        text.font = font;
        return font;
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
    public string FormatSkillText(string text, Dictionary<string, object> placeholders)
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

    public async Task FormatLocalizedText(
        TextMeshProUGUI tmpro,
        string key,
        List<string> placeholderKeys,
        List<string> replacers,
        FontType fontType = FontType.None)
    {
        var langDictionary = Managers.Data.LocalizationDict;
        
        if (langDictionary.TryGetValue(key, out var entryDictionary) == false) return;
        if (entryDictionary.TryGetValue(Language2Letter, out var entry) == false) return;

        var template = entry.Text;
        var formattedText = template;
        
        for (int i = 0; i < replacers.Count; i++)
        {
            string placeholder = "{" + placeholderKeys[i] + "}";
            formattedText = formattedText.Replace(placeholder, replacers[i]);
        }

        tmpro.text = formattedText;
        tmpro.font = fontType == FontType.None ? await GetFont(entry.Font) : await GetFontFromDictionary(fontType);
    }
}
