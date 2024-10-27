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

    private float _valueDistance = 0;                        // 각 페이지 사이의 거리
    private int _maxPage = 0;
    private float _startTouchX;
    private float _endTouchX;
    private readonly float _swipeDistance = 150f;           // 페이지 스와이프를 위해 움직여야 하는 최소 거리
    private int _currentPage = 0;
    
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
        if (IsSwipeMode) return;
        _startTouchX = startX;
    }

    public void EndTouch(float endX)
    {
        if (IsSwipeMode) return;
        _endTouchX = endX;

        if (Math.Abs(_startTouchX - _endTouchX) < _swipeDistance)
        {
            OnPageChanged?.Invoke(CurrentPage);
            return;
        }
        
        bool isLeft = _startTouchX < _endTouchX;
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
