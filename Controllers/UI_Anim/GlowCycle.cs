using UnityEngine;
using UnityEngine.UI;

public class GlowCycle : MonoBehaviour
{
    private Image _img;
    private bool _selected;
    private readonly int _rotSpeed = 60;
    private readonly float _colorChangeTime = 1.0f;
    private readonly float _colorRecoverTime = 2.0f;
    private float _startTime;
    private float _time;
    private float _timer1 = 0f;
    private float _timer2 = 0f;
    
    public bool Selected 
    {
        get => _selected;
        set
        {
            _selected = value;
            if (_selected == false)
            {
                transform.rotation = Quaternion.identity;
                Image img = gameObject.GetComponent<Image>();
                img.color = Color.white;
            }
        }
    } 

    private void Start()
    {
        _img = GetComponent<Image>();
        Selected = false;
        _startTime = Time.time;
    }
    
    private void Update()
    {
        if (!Selected) return;
        
        transform.Rotate(0, 0, _rotSpeed * Time.deltaTime);

        _time = Time.time;
        if (_time < _startTime + _colorRecoverTime)
        {
            if (_time < _startTime + _colorChangeTime)
            {
                _timer1 += Time.deltaTime;
                Color lerpedColor = Color.Lerp(Color.white, Color.yellow, _timer1 / _colorChangeTime);
                _img.color = lerpedColor;
            }
            else
            {
                _timer2 += Time.deltaTime;
                Color lerpedColor = Color.Lerp(Color.yellow, Color.white, _timer2 / _colorRecoverTime);
                _img.color = lerpedColor;
            }
        }
        else
        {
            _startTime = Time.time;
            _timer1 = 0f;
            _timer2 = 0f;
        }
    }
}
