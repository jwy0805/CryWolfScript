using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_MatchMaking : UI_Scene
{
    private Transform _myDeck;
    private Transform _enemyDeck;

    private enum Buttons
    {
        CancelButton,
    }
    
    private enum Images
    {
        LoadingMarkImage,
        EnemyDeckPanel,
        MyDeckPanel,
        VSImage,
    }
    
    private enum Texts
    {
        InfoText,
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        SetButtonEvents();
        SetUI();
    }

    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    protected override void SetButtonEvents()
    {
        GetButton((int)Buttons.CancelButton).gameObject.BindEvent(OnCancelClicked);
    }
    
    protected override void SetUI()
    {
        GetImage((int)Images.EnemyDeckPanel).gameObject.SetActive(false);
        GetImage((int)Images.VSImage).gameObject.SetActive(false);
        SetObjectSize(GetImage((int)Images.LoadingMarkImage).gameObject, 1.0f);
    }

    private void OnCancelClicked(PointerEventData data)
    {
        
    }
}
