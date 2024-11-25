using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using Zenject;
// ReSharper disable ClassNeverInstantiated.Global

public class MainLobbyViewModel
{
    public event Action<int> OnPageChanged;
    public event Action<int> ChangeButtonFocus;

    private float _valueDistance;                        // 각 페이지 사이의 거리
    private int _maxPage;
    private Tuple<float, int> _startTouchX;
    private Tuple<float, int> _endTouchX;
    private readonly float _swipeDistance = 150f;           // 페이지 스와이프를 위해 움직여야 하는 최소 거리
    private int _currentPage;
    
    public float[] ScrollPageValues { get; private set; }
    public bool IsSwipeMode { get; set; } = false;
    public float SwipeTime => 0.2f;
    
    public int CurrentPage 
    { 
        get => _currentPage;
        private set
        {
            _currentPage = value;
            ChangeButtonFocus?.Invoke(value);
        }
    }

    public void OnPlayButtonClicked(UI_MainLobby.GameModeEnums mode)
    {
        switch (mode)
        {
            case UI_MainLobby.GameModeEnums.FriendlyMatch:
                break;
            case UI_MainLobby.GameModeEnums.RankGame:
                LoadMatchMakingScene();
                break;
            case UI_MainLobby.GameModeEnums.SinglePlay:
                break;
        }
    }
    
    private void LoadMatchMakingScene()
    {
        Managers.Scene.LoadScene(Define.Scene.MatchMaking);
    }
    
    // Logics related to the main lobby scroll view
    public void Initialize(int pageCount)
    {
        ScrollPageValues = new float[pageCount];   // 스크롤되는 페이지의 각 value 값을 저장하는 배열 메모리 할당
        _valueDistance = 1f / (ScrollPageValues.Length - 1); // 스크롤되는 페이지 사이의 거리
        
        for (int i = 0; i < ScrollPageValues.Length; i++)   // 스크롤되는 페이지의 각 value 위치 설정 [0 <= value <= 1]
        {
            ScrollPageValues[i] = i * _valueDistance;
        }

        _maxPage = pageCount;           
    }
    
    public void StartTouch(float startX)
    {
        _startTouchX = new Tuple<float, int>(startX, Managers.UI.PopupList.Count);
    }

    public void EndTouch(float endX)
    {
        _endTouchX = new Tuple<float, int>(endX, Managers.UI.PopupList.Count);

        if (IsSwipeMode || Managers.UI.PopupList.Count > 0) return;
        if (_startTouchX.Item2 != _endTouchX.Item2) return;
        if (Math.Abs(_startTouchX.Item1 - _endTouchX.Item1) < _swipeDistance)
        {
            OnPageChanged?.Invoke(CurrentPage);
            return;
        }
        
        bool isLeft = _startTouchX.Item1 < _endTouchX.Item1;
        if (isLeft)
        {
            if (CurrentPage == 0) return;
            CurrentPage--;
        }
        else
        {
            if (CurrentPage == _maxPage - 1) return;
            CurrentPage++;
        }
        
        OnPageChanged?.Invoke(CurrentPage);
    }
    
    public float GetScrollPageValue(int index)
    {
        return ScrollPageValues[index];
    }
    
    public void SetCurrentPage(int index)
    {
        CurrentPage = index;
        OnPageChanged?.Invoke(index);
    }
}
