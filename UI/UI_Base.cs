using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public abstract class UI_Base : MonoBehaviour
{
    protected readonly Dictionary<Type, Object[]> Objects = new();

    protected abstract void Init();
    protected virtual void BindObjects() { }
    protected virtual void InitButtonEvents() { }
    protected virtual void InitBackgroundSize(RectTransform rectTransform) { }
    protected virtual void InitUI() { }
    
    private void Start()
    {
        Init();
    }

    protected void Bind<T>(Type type) where T : Object
    {
        string[] names = Enum.GetNames(type);
        Object[] objects = new Object[names.Length];
        Objects.Add(typeof(T), objects);
        
        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
            {
                objects[i] = Util.FindChild(gameObject, names[i], true);
            }
            else
            {
                objects[i] = Util.FindChild<T>(gameObject, names[i], true);
            }

            if (objects[i] == null)
            {
                Debug.Log($"Failed to bind({names[i]}");
            }
        }
    }
    
    // If needed to use both methods - BindData<T> and Bind<T> - BindData<T> should be called first
    protected virtual void BindData<T>(Type enumType, Dictionary<string, GameObject> dict) where T : Object
    {
        Bind<T>(enumType);
        
        if (typeof(T) == typeof(Button))
        {
            for (int i = 0; i < Objects[typeof(T)].Length; i++)
            {
                GameObject btn = GetButton(i).gameObject;
                dict.Add(btn.name, btn);
            }
        }
        else if (typeof(T) == typeof(Image))
        {
            for (int i = 0; i < Objects[typeof(T)].Length; i++)
            {
                GameObject img = GetImage(i).gameObject;
                dict.Add(img.name, img);
            }
        }
        else if (typeof(T) == typeof(TextMeshProUGUI))
        {
            for (int i = 0; i < Objects[typeof(T)].Length; i++)
            {
                GameObject txt = GetText(i).gameObject;
                dict.Add(txt.name, txt);
            }
        }
        
        Objects.Clear();
    }

    private T Get<T>(int idx) where T : Object
    {
        if (Objects.TryGetValue(typeof(T), out var objects) == false)
        {
            return null;
        }

        return objects[idx] as T;
    }
    
    protected float GetObjectSide(Rect rect)
    {
        float width = rect.width;
        float height = rect.height;
        
        return width < height ? width : height;
    }
    
    protected GameObject GetObject(int idx) { return Get<GameObject>(idx); }
    protected TextMeshProUGUI GetText(int idx) { return Get<TextMeshProUGUI>(idx); }
    protected TMP_InputField GetTextInput(int idx) { return Get<TMP_InputField>(idx); }
    protected Button GetButton(int idx) { return Get<Button>(idx); }
    protected Image GetImage(int idx) { return Get<Image>(idx); }

    public static void BindEvent(GameObject go, Action<PointerEventData> action,
        Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

        switch (type)
        {
            case Define.UIEvent.Click:
                evt.OnClickHandler -= action;
                evt.OnClickHandler += action;
                break;
            case Define.UIEvent.Drag:
                evt.OnDragHandler -= action;
                evt.OnDragHandler += action;
                break;
        }
    }
    
    // Square
    protected void SetObjectSize(GameObject go, float sizeParam = 1.0f)
    {
        Transform parent = go.transform.parent;
        RectTransform rt = go.GetComponent<RectTransform>();
        RectTransform rtParent = parent.GetComponent<RectTransform>();
        Rect rect = rtParent.rect;
        
        float width = rect.width;
        float height = rect.height; 
        float side = width < height ? width : height;

        rt.sizeDelta = new Vector2(side * sizeParam, side * sizeParam);
    }
    
    protected void SetObjectSize(GameObject go, float sizeParam, bool inScrollView)
    {
        Transform parent = go.transform.parent;
        RectTransform rt = go.GetComponent<RectTransform>();
        RectTransform rtParent = parent.GetComponent<RectTransform>();
        Vector2 size = rtParent.sizeDelta;
        
        float width = size.x;
        float height = size.y;
        float side = width < height ? width : height;
        
        rt.sizeDelta = new Vector2(side * sizeParam, side * sizeParam);
    }
    
    // Rectangular
    protected void SetObjectSize(GameObject go, float xParam, float yParam)
    {
        Transform parent = go.transform.parent;
        RectTransform rt = go.GetComponent<RectTransform>();
        RectTransform rtParent = parent.GetComponent<RectTransform>();
        Rect rect = rtParent.rect;
        
        float width = rect.width;
        float height = rect.height; 
        float side = width < height ? width : height;

        rt.sizeDelta = new Vector2(side * xParam, side * yParam);
    }
    
    protected void SetLineSize(GameObject go, float yParam = 1.0f)
    {
        Transform parent = go.transform.parent;
        RectTransform rt = go.GetComponent<RectTransform>();
        RectTransform rtParent = parent.GetComponent<RectTransform>();
        Rect rect = rtParent.rect;
        
        float width = rect.width;
        float height = rect.height; 
        float side = width < height ? width : height;

        rt.sizeDelta = new Vector2(10, side * yParam);
    }

    protected void SetSkillPanel(string skillPanel, Transform parent)
    {
        GameObject go = Managers.Resource.Instantiate($"UI/Skill/{skillPanel}");
        go.transform.SetParent(parent);
        RectTransform rectTransform = go.GetComponent<RectTransform>();
        rectTransform.offsetMax = new Vector2(0.0f, 0.0f);
        rectTransform.offsetMin = new Vector2(0.0f, 0.0f);
    }
}
