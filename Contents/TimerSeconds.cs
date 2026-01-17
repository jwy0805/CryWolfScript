using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerSeconds : MonoBehaviour
{
    private DateTime _lastRefreshTime;
    private DateTime _refreshTime;
    private Coroutine _timerCoroutine;
    
    public Image AdsImage { get; set; }
    public TextMeshProUGUI TimerText { get; set; }

    public DateTime LastRefreshTime
    {
        get => _lastRefreshTime;
        set
        {
            _lastRefreshTime = value;

            var duration = GetDurationUntilCycleEnd(_lastRefreshTime);
            _refreshTime = _lastRefreshTime + duration;
            
            if (TimerText != null && AdsImage != null)
            {
                var parent = TimerText.transform.parent.gameObject;
                
                if (_refreshTime <= DateTime.UtcNow)
                {
                    parent.SetActive(false);
                    AdsImage.gameObject.SetActive(true);
                    return;
                }
                
                parent.SetActive(true);
                AdsImage.gameObject.SetActive(false);
                
                var timeLeft = _refreshTime - DateTime.UtcNow;
                TimerText.text = FormatHms(timeLeft);
            }
            
            if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
            _timerCoroutine = StartCoroutine(UpdateTimerCoroutine());
        }
    }

    private TimeSpan GetDurationUntilCycleEnd(DateTime lastRefresh)
    {
        var lastRefreshUtc = lastRefresh.Kind switch
        {
            DateTimeKind.Utc => lastRefresh,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(lastRefresh, DateTimeKind.Utc),
            DateTimeKind.Local => lastRefresh.ToUniversalTime(),
            _ => DateTime.SpecifyKind(lastRefresh, DateTimeKind.Utc)
        };
        
        var sixHours = TimeSpan.FromHours(6);
        var nextUtcMidnight = new DateTime(lastRefreshUtc.Year, lastRefreshUtc.Month, lastRefreshUtc.Day, 
            0, 0, 0, DateTimeKind.Utc).AddDays(1);
        var untilNextUtcMidnight = nextUtcMidnight - lastRefreshUtc;
     
        if (untilNextUtcMidnight <= TimeSpan.Zero)
            return TimeSpan.Zero;

        return (untilNextUtcMidnight < sixHours) ? untilNextUtcMidnight : sixHours;
    }
    
    private IEnumerator UpdateTimerCoroutine()
    {
        while (true)
        {
            if (TimerText == null) yield break;

            var timeLeft = _refreshTime - DateTime.UtcNow;
            
            if (timeLeft.TotalSeconds <= 0)
            {
                TimerText.transform.parent.gameObject.SetActive(false);
                yield break;
            }

            TimerText.text = FormatHms(timeLeft);
            yield return new WaitForSeconds(1f);
        }
    }

    private string FormatHms(TimeSpan ts)
    {
        if (ts < TimeSpan.Zero) ts = TimeSpan.Zero;

        var hours = (int)ts.TotalHours; // 6시간보다 작을 수도 있음
        var minutes = ts.Minutes;
        var seconds = ts.Seconds;

        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }
    
    private void OnDisable()
    {
        if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
    }
}
