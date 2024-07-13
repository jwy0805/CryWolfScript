using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_MatchMaking : UI_Scene
{
    private Transform _myDeck;
    private Transform _enemyDeck;
    private RectTransform _loadingMark;
    private bool _loadingMarkActive;

    private enum Buttons
    {
        CancelButton,
    }
    
    private enum Images
    {
        LoadingMarkImage,
        MyDeckPanel,
        EnemyDeckPanel,
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
        
        StartMatchMaking();
    }

    protected void Update()
    {
        if (_loadingMarkActive) _loadingMark.Rotate(0, 0, 180 * Time.deltaTime);
    }

    private void StartMatchMaking()
    {
        var changeActPacket = new ChangeActPacketRequired
        {
            AccessToken = Managers.User.AccessToken,
            Act = UserAct.MatchMaking
        };
        
        Managers.Web.SendPutRequest<ChangeActPacketResponse>(
            "UserAccount/ChangeAct", changeActPacket, response =>
            {
                if (response.ChangeOk)
                {
                    Managers.Network.ConnectGameSession();
                }
            });
        
        SetDeckInfo();
    }

    private void SetDeckInfo()
    {
        _myDeck = GetImage((int)Images.MyDeckPanel).transform;
        var deck = Util.Camp == Camp.Sheep ? Managers.User.DeckSheep : Managers.User.DeckWolf;
        foreach (var unit in deck.UnitsOnDeck)
        {
            Util.GetCardResources(unit, _myDeck, 150);
        }
    }
    
    public void SetEnemyInfo()
    {
        _loadingMarkActive = false;
        
        GetButton((int)Buttons.CancelButton).gameObject.SetActive(false);
        GetImage((int)Images.LoadingMarkImage).gameObject.SetActive(false);
        GetImage((int)Images.EnemyDeckPanel).gameObject.SetActive(true);
        GetImage((int)Images.VSImage).gameObject.SetActive(true);
    }
    
    private void SomeMethod()
    {
        Managers.Map.MapId = 1;
        Managers.Scene.LoadScene(Define.Scene.Game);
        Managers.Clear();
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
        SetObjectSize(GetImage((int)Images.LoadingMarkImage).gameObject, 0.2f);
        
        _loadingMark = GetImage((int)Images.LoadingMarkImage).rectTransform;
        _loadingMarkActive = true;
    }

    private void OnCancelClicked(PointerEventData data)
    {
        var changeActPacket = new ChangeActPacketRequired
        {
            AccessToken = Managers.User.AccessToken,
            Act = UserAct.InLobby
        };
        
        Managers.Web.SendPutRequest<ChangeActPacketResponse>(
            "UserAccount/ChangeAct", changeActPacket, response =>
            {
                if (response.ChangeOk)
                {
                    Managers.Network.ConnectGameSession();
                }
            });
        
        Managers.Scene.LoadScene(Define.Scene.MainLobby);
        Managers.Clear();
    }
}
