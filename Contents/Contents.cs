using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

public class Contents
{
    [Serializable]
    public class UnitData
    {
        [Preserve] public int Id { get; set; }
        [Preserve] public string Name { get; set; }
        [Preserve] public Faction Faction { get; set; }
        [Preserve] public Role UnitRole { get; set; }
        [Preserve] public UnitClass UnitClass { get; set; }
        [Preserve] public Species UnitSpecies { get; set; }
        [Preserve] public UnitRegion Region { get; set; }
        [Preserve] public string RecommendedLocation { get; set; }
        [Preserve] public StatInfo Stat { get; set; }
    }
    
    [Serializable]
    public class ObjectData
    {
        [Preserve] public int Id { get; set; }
        [Preserve] public string Name { get; set; }
        [Preserve] public StatInfo Stat { get; set; }
    }

    [Serializable]
    public class SkillData
    {
        [Preserve] public int Id { get; set; }
        [Preserve] public string Explanation { get; set; }
        [Preserve] public float Value { get; set; }
        [Preserve] public float Coefficient { get; set; }
        [Preserve] public int Cost { get; set; }
    }

    [Serializable]
    public class TutorialData
    {
        [Preserve] public TutorialType Type { get; set; }
        [Preserve] public List<TutorialStep> Steps { get; set; }
        [NonSerialized] public Dictionary<string, TutorialStep> StepsDict;
        
        // StepDict 초기화
        public void Initialize()
        {
            StepsDict = Steps.ToDictionary(step => step.Tag);
        }
    }

    [Serializable]
    public class TutorialStep
    {
        [Preserve] public string Tag { get; set; }
        [Preserve] public string DialogKey { get; set; }
        [Preserve] public string Speaker { get; set; }
        [Preserve] public List<string> Actions { get; set; }
        [Preserve] public string Next { get; set; }
    }
    
    [Serializable]
    public class LocalizationEntry
    {
        [Preserve] public string Text { get; set; }
        [Preserve] public string Font { get; set; }
        [Preserve] public int FontSize { get; set; }
    }

    [Serializable]
    public class LocalizationItem
    {
        [Preserve] public string Key { get; set; }
        [Preserve] public Dictionary<string, LocalizationEntry> Translations { get; set; }
    }
    
    [Serializable]
    public class UnitLoader : ILoader<int, UnitData>
    {
        [Preserve] public List<UnitData> Units { get; set; } = new();
    
        [Preserve]
        public Dictionary<int, UnitData> MakeDict()
        {
            return Units.ToDictionary(unit => unit.Id);
        }
    }

    [Serializable]
    public class ObjectLoader : ILoader<int, ObjectData>
    {
        [Preserve] public List<ObjectData> Objects { get; set; } = new();
        
        [Preserve]
        public Dictionary<int, ObjectData> MakeDict()
        {
            return Objects.ToDictionary(player => player.Id);
        }
    }

    [Serializable]
    public class SkillLoader : ILoader<int, SkillData>
    {
        [Preserve] public List<SkillData> Skills { get; set; } = new();

        [Preserve]
        public Dictionary<int, SkillData> MakeDict()
        {
            return Skills.ToDictionary(skill => skill.Id);
        }
    }
    
    [Serializable]
    public class TutorialLoader : ILoader<TutorialType, TutorialData>
    {
        [Preserve] public List<TutorialData> Tutorials { get; set; } = new();
        
        [Preserve]
        public Dictionary<TutorialType, TutorialData> MakeDict()
        {
            var dict = new Dictionary<TutorialType, TutorialData>();
            foreach (var tutorial in Tutorials)
            {
                tutorial.Initialize();
                dict[tutorial.Type] = tutorial;
            }

            return dict;
        }
    }
}
