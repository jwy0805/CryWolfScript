using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_TouchToStart : UI_Scene
{
   protected override void Init()
   {
      base.Init();

      InitUI();
   }
   
   protected override void InitBackgroundSize(RectTransform rectTransform)
   {
      Rect rect = GameObject.Find("UI_TouchToStart").GetComponent<RectTransform>().rect;
      float canvasWidth = rect.width;
      float canvasHeight = rect.height;
      float backgroundHeight = canvasWidth * 1.2f;
      float nightSkyHeight = canvasHeight - backgroundHeight;
        
      RectTransform rtBackground = GameObject.Find("Background").GetComponent<RectTransform>();
      rtBackground.sizeDelta = new Vector2(canvasWidth, backgroundHeight);

      RectTransform rtNightSky = GameObject.Find("NightSky").GetComponent<RectTransform>();
      rtNightSky.sizeDelta = new Vector2(canvasWidth, nightSkyHeight);
   }

   protected override void InitUI()
   {
      InitBackgroundSize(gameObject.GetComponent<RectTransform>());
   }
}
