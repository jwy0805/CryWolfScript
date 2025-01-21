using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UI_TutorialPopup : UI_Popup
{
    private TutorialViewModel _tutorialVm;
    
    private TutorialNpcInfo _tutorialNpcInfo;

    private enum Images
    {
        ContinueButtonLine,
    }

    private enum Buttons
    {
        ContinueButton,
    }

    private enum Texts
    {
        ContinueButtonText,
    }
    
    [Inject]
    public void Construct(TutorialViewModel tutorialViewModel)
    {
        _tutorialVm = tutorialViewModel;
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
        Bind<Button>(typeof(Buttons)); 
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    protected override void InitButtonEvents()
    {
        
    }
    
    protected override void InitUI()
    {
        var tutorialNpc = Managers.Resource.Instantiate("Npc/NpcWerewolf");
        _tutorialNpcInfo = tutorialNpc.GetComponent<TutorialNpcInfo>();
        _tutorialVm.InitTutorialCamera(_tutorialNpcInfo.Position, _tutorialNpcInfo.CameraPosition);
        
        StartCoroutine(nameof(SmoothAlphaRoutine));
    }
    
    private IEnumerator SmoothAlphaRoutine()
    {
        float highAlpha = 1f;           
        float lowAlpha = 120f / 255f;  
        float duration = 1f;         

        while (true)
        {
            // 1) highAlpha -> lowAlpha
            yield return StartCoroutine(LerpAlpha(highAlpha, lowAlpha, duration));

            // 2) lowAlpha -> highAlpha
            yield return StartCoroutine(LerpAlpha(lowAlpha, highAlpha, duration));
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private IEnumerator LerpAlpha(float from, float to, float duration)
    {
        float elapsed = 0f;
        var targetImage = GetImage((int)Images.ContinueButtonLine);
        var targetText = GetText((int)Texts.ContinueButtonText);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentAlpha = Mathf.Lerp(from, to, t);
            
            SetImageAlpha(targetImage, currentAlpha);
            SetTextAlpha(targetText, currentAlpha);

            yield return null;
        }
    }

    private void SetImageAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;
        var color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    private void SetTextAlpha(TextMeshProUGUI text, float alpha)
    {
        if (text == null) return;
        var color = text.color;
        color.a = alpha;
        text.color = color;
    }
}
