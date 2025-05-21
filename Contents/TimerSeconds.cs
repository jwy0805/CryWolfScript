using System;
using TMPro;
using UnityEngine;

public class TimerSeconds : MonoBehaviour
{
    private DateTime _lastRefreshTime;
    private DateTime _refreshTime;
    private DateTime _currentTime;
    
    public TextMeshProUGUI TimerText { get; set; }

    public DateTime LastRefreshTime
    {
        get => _lastRefreshTime;
        set
        {
            _lastRefreshTime = value;
            _refreshTime = _lastRefreshTime.AddHours(6);
            
            if (TimerText != null)
            {
                TimerText.gameObject.SetActive(true);
                TimerText.text = "06:00:00";
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        _currentTime = DateTime.UtcNow;
        var timeLeft = _refreshTime - _currentTime;

        if (timeLeft.TotalSeconds <= 0)
        {
            TimerText.gameObject.SetActive(false);
            return;
        }

        var hours = (int)timeLeft.TotalHours;
        var minutes = (int)timeLeft.TotalMinutes;
        var seconds = timeLeft.Seconds;

        TimerText.text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }
}
