using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TimerSeconds : MonoBehaviour
{
    private DateTime _lastRefreshTime;
    private DateTime _refreshTime;
    private Coroutine _timerCoroutine;
    
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
                if (_refreshTime <= DateTime.UtcNow)
                {
                    TimerText.transform.parent.gameObject.SetActive(false);
                    return;
                }
                
                TimerText.transform.parent.gameObject.SetActive(true);
                TimerText.text = "06:00:00";
            }
            
            if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
            _timerCoroutine = StartCoroutine(UpdateTimerCoroutine());
        }
    }

    private IEnumerator UpdateTimerCoroutine()
    {
        while (true)
        {
            if (TimerText == null) yield break;

            var currentTime = DateTime.UtcNow;
            var timeLeft = _refreshTime - currentTime;
            
            if (timeLeft.TotalSeconds <= 0)
            {
                TimerText.transform.parent.gameObject.SetActive(false);
                yield break;
            }
            
            var hours = (int)timeLeft.TotalHours;
            var minutes = timeLeft.Minutes;
            var seconds = timeLeft.Seconds;
            
            TimerText.text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnDisable()
    {
        if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
    }
}
