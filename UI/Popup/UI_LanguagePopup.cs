using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_LanguagePopup : UI_Popup
{
    private MainLobbyViewModel _lobbyVm;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    private readonly Dictionary<string, GameObject> _buttonDict = new();
    private GameObject _currentLanguageButton;
    
    public UI_SettingsPopup SettingsPopup { get; set; }

    private enum Buttons
    {
        EnButton,
        KoButton,
        JaButton,
        ViButton,
        ExitButton,
    }
    
    private enum Texts
    {
        LanguageTitleText,
    }

    [Inject]
    public void Construct(MainLobbyViewModel lobbyVm)
    {
        _lobbyVm = lobbyVm;
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();
    }

    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        
        _buttonDict.TryAdd("en", GetButton((int)Buttons.EnButton).gameObject);
        _buttonDict.TryAdd("ko", GetButton((int)Buttons.KoButton).gameObject);
        _buttonDict.TryAdd("ja", GetButton((int)Buttons.JaButton).gameObject);
        _buttonDict.TryAdd("vi", GetButton((int)Buttons.ViButton).gameObject);
        
        _ = Managers.Localization.UpdateTextAndFont(_textDict);
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitClicked);
        foreach (var go in _buttonDict.Values)
        {
            go.BindEvent(OnLanguageClicked);
        }
    }

    protected override void InitUI()
    {
        SetLanguageButton(Managers.Localization.Language2Letter);
    }

    private void SetLanguageButton(string language2Letter)
    {
        _currentLanguageButton =  _buttonDict[language2Letter];
        
        foreach (var go in _buttonDict.Values)
        {
            var mark = Util.FindChild(go, "IconCheck", true, true);
            if (go.name == _currentLanguageButton.name)
            {
                mark.SetActive(true);
                go.GetComponent<Image>().color = Color.white;
            }
            else
            {
                mark.SetActive(false);
                go.GetComponent<Image>().color = Color.gray;
            }
        }
    }
    
    private async Task OnLanguageClicked(PointerEventData data)
    {
        var clickedButton = data.pointerPress.gameObject;
        var language2Letter = clickedButton.name.Replace("Button", "").ToLower();
        
        await ChangeLanguageAsync(language2Letter);
    }
    
    private async Task ChangeLanguageAsync(string language2Letter)
    {
        Managers.Localization.SetLanguage(language2Letter);
        await Task.WhenAll(SettingsPopup.UpdateFlag(), SettingsPopup.ChangeLanguage(), _lobbyVm.ChangeLanguage());
        
        Managers.UI.ClosePopupUI();
    }
    
    private void OnExitClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }
}
