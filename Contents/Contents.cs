using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using Unity.Plastic.Newtonsoft.Json;
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
        public StatInfo Stat { get; set; }
    }

    [Serializable]
    public class SkillData
    {
        public int Id { get; set; }
        public string Explanation { get; set; }
        public int Cost { get; set; }
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
}
