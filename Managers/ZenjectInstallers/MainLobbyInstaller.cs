using UnityEngine;
using Zenject;

public class MainLobbyInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<MainLobbyViewModel>().AsSingle();
        Container.Bind<DeckViewModel>().AsSingle();
        Container.Bind<CollectionViewModel>().AsSingle();
        Container.Bind<CraftingViewModel>().AsSingle();
        Container.Bind<ShopViewModel>().AsSingle();
        Container.Bind<TutorialViewModel>().AsSingle();
    }
}
