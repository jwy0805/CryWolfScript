using UnityEngine;
using Zenject;

public class SinglePlayInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<SinglePlayViewModel>().AsSingle();
        Container.Bind<DeckViewModel>().AsSingle();
    }
}
