using UnityEngine;
using Zenject;

public class FriendlyMatchInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<MainLobbyViewModel>().AsSingle();
        Container.Bind<FriendlyMatchViewModel>().AsSingle();
        Container.Bind<MatchMakingViewModel>().AsSingle();
        Container.Bind<DeckViewModel>().AsSingle();
    }
}
