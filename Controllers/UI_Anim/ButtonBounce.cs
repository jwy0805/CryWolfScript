using UnityEngine;

public class ButtonBounce : MonoBehaviour
{
    private RectTransform _rectTransform;
    private bool _selected;
    private float _time = 0f;
    private float _delta = 0f;

    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            if (_selected == false)
            {
                SetTopAndBottom(0, 0);
                return;
            }
            _time = Time.time;
            _delta = 0;
        }
    }
    
    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        Selected = false;
    }

    private void Update()
    {
        if (Selected == false) return;
        if (Time.time < _time + 0.4f)
        {
            SetTopAndBottom(50 * _delta, 50 * _delta);
        }
        else if (Time.time < _time + 0.8f)
        {
            SetTopAndBottom(20 * _delta, 0 * _delta);
        }
        else if (Time.time < _time + 1.2f)
        {
            SetTopAndBottom(20 * _delta, 20 * _delta);
        }
        else
        {
            SetTopAndBottom(0 * _delta, 0 * _delta);
        }

        _delta += Time.deltaTime;
    }

    private void SetTopAndBottom(float topValue, float bottomValue)
    {
        _rectTransform.offsetMax = new Vector2(_rectTransform.offsetMax.x, topValue);
        _rectTransform.offsetMin = new Vector2(_rectTransform.offsetMin.x, bottomValue);
    }
}
