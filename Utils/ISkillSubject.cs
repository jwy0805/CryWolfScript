using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISkillSubject
{
    void AddObserver(ISkillObserver observer);
    void RemoveObserver(ISkillObserver observer);
    void Notify();
}
