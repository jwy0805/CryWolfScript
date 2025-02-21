using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Serialization;

public class Contents
{
    [Serializable]
    public class UnitData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Faction Faction { get; set; }
        public Role UnitRole { get; set; }
        public UnitClass UnitClass { get; set; }
        public Species UnitSpecies { get; set; }
        public UnitRegion Region { get; set; }
        public string RecommendedLocation { get; set; }
        public StatInfo Stat { get; set; }
    }
    
    [Serializable]
    public class ObjectData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public StatInfo Stat { get; set; }
    }

    [Serializable]
    public class SkillData
    {
        public int Id { get; set; }
        public string Explanation { get; set; }
        public int Value { get; set; }
        public float Coefficient { get; set; }
        public int Cost { get; set; }
    }

    [Serializable]
    public class TutorialData
    {
        public TutorialType Type { get; set; }
        public List<TutorialStep> Steps { get; set; }
    }

    [Serializable]
    public class TutorialStep
    {
        public int Step { get; set; }
        public string DialogKey { get; set; }
        public string Speaker { get; set; }
        public string BubblePosition { get; set; }
        public List<string> Events { get; set; }
    }
    
    [Serializable]
    public class LocalizationEntry
    {
        public string Text { get; set; }
        public string Font { get; set; }
        public int FontSize { get; set; }
    }

    [Serializable]
    public class LocalizationItem
    {
        public string Key { get; set; }
        public Dictionary<string, LocalizationEntry> Translations { get; set; }
    }
    
    [Serializable]
    public class UnitLoader : ILoader<int, UnitData>
    {
        public List<UnitData> Units { get; set; } = new();
    
        public Dictionary<int, UnitData> MakeDict()
        {
            return Units.ToDictionary(unit => unit.Id);
        }
    }

    [Serializable]
    public class ObjectLoader : ILoader<int, ObjectData>
    {
        public List<ObjectData> Objects { get; set; } = new();
    
        public Dictionary<int, ObjectData> MakeDict()
        {
            return Objects.ToDictionary(player => player.Id);
        }
    }

    [Serializable]
    public class SkillLoader : ILoader<int, SkillData>
    {
        public List<SkillData> Skills { get; set; } = new();

        public Dictionary<int, SkillData> MakeDict()
        {
            return Skills.ToDictionary(skill => skill.Id);
        }
    }
    
    [Serializable]
    public class TutorialLoader : ILoader<TutorialType, TutorialData>
    {
        public List<TutorialData> Tutorials { get; set; } = new();
        
        public Dictionary<TutorialType, TutorialData> MakeDict()
        {
            return Tutorials.ToDictionary(tutorial => tutorial.Type);
        }
    }
}
