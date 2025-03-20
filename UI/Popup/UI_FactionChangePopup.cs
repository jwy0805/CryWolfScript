using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_FactionChangePopup : UI_Popup
{
    private TutorialViewModel _tutorialVm;

    private GameObject _tutorialNpc;
    private GameObject _continueButton;
    private GameObject _hand;
    private GameObject _speakerPanel;
    private RectTransform _handRect;
    private TextMeshProUGUI _speechBubbleText;

    private enum Images
    {
        ContinueButtonLine,
        LeftPanel,
        Hand,
    }

    private enum Buttons
    {
        ExitButton,
    }

    private enum Texts
    {
        ContinueButtonText
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
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        _hand = GetImage((int)Images.Hand).gameObject;
        _handRect = _hand.GetComponent<RectTransform>();
        _speakerPanel = GetImage((int)Images.LeftPanel).gameObject;
        _speechBubbleText = Util.FindChild(_speakerPanel, "SpeechBubbleText", true).GetComponent<TextMeshProUGUI>();
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnExitButtonClicked);
    }

    protected override void InitUI()
    {
        _tutorialNpc = GameObject.FindGameObjectWithTag("Npc");
        if (_tutorialNpc == null)
        {
            _tutorialNpc = Managers.Resource.Instantiate("Npc/NpcWerewolf");
        }
        
        var npcInfo = _tutorialNpc.GetComponent<TutorialNpcInfo>();
        var npcPos = npcInfo.Position;
        var cameraPos = npcInfo.CameraPosition;
        
        _tutorialVm.InitTutorialChangeFaction(npcPos, cameraPos);
        
        SetText();
        StartCoroutine(PointToFactionButton());
    }

    private void SetText()
    {
        const string key = "tutorial_faction_change_popup_text";
        var textContent = Managers.Localization.GetLocalizedValue(_speechBubbleText, key);
        _speechBubbleText.text = textContent;
    }
    
    #region UI Effects
    
    // The offset for moving the finger from its original position (in pixels on the UI).
    // For example, setting (30, -30) will move it diagonally towards the bottom-right.
    private IEnumerator HandPokeRoutine(Vector2 offset, float moveDuration, float holdTime, float waitTime, int repeat = 3)
    {
        Vector2 originalPos = _handRect.anchoredPosition;
        Vector2 targetPos = originalPos + offset;
        
        for (int i = 0; i < repeat; i++)
        {
            // 목표 위치로 부드럽게 이동
            float elapsed = 0f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);
                _handRect.anchoredPosition = Vector2.Lerp(originalPos, targetPos, t);
                yield return null;
            }

            // 목표 위치에서 잠시 대기
            yield return new WaitForSeconds(holdTime);

            // 원래 위치로 부드럽게 복귀
            elapsed = 0f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);
                _handRect.anchoredPosition = Vector2.Lerp(targetPos, originalPos, t);
                yield return null;
            }

            // 다음 동작 전에 대기
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    #endregion

    private IEnumerator PointToFactionButton()
    {
        var anchor = new Vector2(0.25f, 0.85f);
        var offset = new Vector2(-60, 60);
        
        _handRect.anchorMin = anchor;
        _handRect.anchorMax = anchor;
        
        yield return StartCoroutine(HandPokeRoutine(offset, 0.5f, 0.1f, 0.2f));
    }
    
    private void OnExitButtonClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }
}
