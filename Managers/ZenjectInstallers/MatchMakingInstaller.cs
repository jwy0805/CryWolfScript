using UnityEngine;
using Zenject;

public class MatchMakingInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<MatchMakingViewModel>().AsSingle();
        Container.Bind<DeckViewModel>().AsSingle();
    }
}
