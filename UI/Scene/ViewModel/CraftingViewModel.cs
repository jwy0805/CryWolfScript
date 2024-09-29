using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using Zenject;

public class CraftingViewModel
{
    public event Action GoToCraftingTab;

    public void OnCraftingButtonClicked()
    {
        GoToCraftingTab?.Invoke();
    }
    
    
}