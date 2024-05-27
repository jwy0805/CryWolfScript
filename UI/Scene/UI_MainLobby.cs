using System.Collections;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UI_MainLobby : UI_Scene
{
    [SerializeField] private Scrollbar scrollbar;           // 스크롤바의 위치를 바탕으로 현재 페이지 검사
    [SerializeField] private float swipeTime = 0.2f;        // 페이지가 스와이프되는 시간
    [SerializeField] private float swipeDistance = 150f;     // 페이지 스와이프를 위해 움직여야 하는 최소 거리

    private float[] _scrollPageValues;                       // 각 페이지의 위치 값 [0.0 - 1.0]
    private float _valueDistance = 0;                        // 각 페이지 사이의 거리
    private int _currentPage = 0;                        
    private int _maxPage = 0;
    private float _startTouchX;
    private float _endTouchX;
    private bool _isSwipeMode = false;                       // 현재 스와이프가 되고 있는지 체크

    private void Awake()
    {
        var content = Util.FindChild(gameObject, "HorizontalContents", true).transform;
        _scrollPageValues = new float[content.childCount];   // 스크롤되는 페이지의 각 value 값을 저장하는 배열 메모리 할당
        _valueDistance = 1f / (_scrollPageValues.Length - 1); // 스크롤되는 페이지 사이의 거리
        
        for (int i = 0; i < _scrollPageValues.Length; i++)   // 스크롤되는 페이지의 각 value 위치 설정 [0 <= value <= 1]
        {
            _scrollPageValues[i] = i * _valueDistance;
        }

        _maxPage = content.childCount;                     
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        SetButtonEvents();
        
        Util.Camp = Camp.Sheep;
        SetUI();
        SetScrollbarValue(2);
    }

    private void Update()
    {
        UpdateInput();
    }

    private void UpdateInput()
    {
        if (_isSwipeMode) return;
        if (Input.GetMouseButtonDown(0))
        {
            _startTouchX = Input.mousePosition.x;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _endTouchX = Input.mousePosition.x;
            UpdateSwipe();
        }
    }
    
    private void SetScrollbarValue(int index)
    {
        _currentPage = index;
        scrollbar.value = _scrollPageValues[index];
    }

    private void UpdateSwipe()
    {
        if (Mathf.Abs(_startTouchX - _endTouchX) < swipeDistance)
        {
            StartCoroutine(nameof(OnSwipeOneStep), _currentPage);
            return;
        }

        bool isLeft = _startTouchX < _endTouchX;
        if (isLeft)
        {
            if (_currentPage == 0) return;
            _currentPage--;
        }
        else
        {
            if (_currentPage == _maxPage - 1) return;
            _currentPage++;
        }
        
        StartCoroutine(nameof(OnSwipeOneStep), _currentPage);
    }

    private IEnumerator OnSwipeOneStep(int index)
    {
        float start = scrollbar.value;
        float current = 0;
        float percent = 0;

        _isSwipeMode = true;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / swipeTime;
            
            scrollbar.value = Mathf.Lerp(start, _scrollPageValues[index], percent);

            yield return null;
        }
        
        _isSwipeMode = false;
    }
    
    private void OnCampButtonClicked(PointerEventData data)
    {
        Util.Camp = Util.Camp == Camp.Sheep ? Camp.Wolf : Camp.Sheep;
        Debug.Log(Util.Camp);
        SwitchDeck(Util.Camp);
        SwitchCollection(Util.Camp);
        SwitchLobbyUI(Util.Camp);
    }

    private void OnSingleClicked(PointerEventData data)
    {
        
    }

    private void OnMultiClicked(PointerEventData data)
    {
        Managers.UI.ShowPopupUI<UI_Dim>();
        Managers.UI.ShowPopupUI<UI_BattlePopupSheep>();
        
        Util.Deck = Util.Camp == Camp.Sheep ? Managers.User.DeckSheep : Managers.User.DeckWolf;
    }
}
