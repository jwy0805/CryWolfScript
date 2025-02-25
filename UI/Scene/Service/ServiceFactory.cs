using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IServiceFactory
{
    void Bind(DiContainer container);
}

public class ServiceInstaller
{
    private static bool _servicesBound;
    
    public void CreateFactory(string path, DiContainer container)
    {
        switch (path)
        {
            case "UI/Scene/UI_Login":
                new LoginServiceFactory().Bind(container);
                break;
            case "UI/Scene/UI_MainLobby":
                new MainLobbyServiceFactory().Bind(container);
                break;
            case "UI/Scene/UI_MatchMaking":
                new MatchMakingServiceFactory().Bind(container);
                break;
            case "UI/Scene/UI_FriendlyMatch":
                new FriendlyMatchServiceFactory().Bind(container);
                break;
            case "UI/Scene/UI_SinglePlay":
                new SinglePlayServiceFactory().Bind(container);
                break;
            case "UI/Scene/UI_GameSingleWay":
                new GameServiceFactory().Bind(container);
                break;
        }
    }

    public void CreateFactoryOnProjectContext(DiContainer container)
    {
        if (_servicesBound) return;
        
        container.Bind<IUserService>().To<UserService>().AsSingle();
        container.Bind<IWebService>().To<WebService>().AsSingle();
        container.Bind<ITokenService>().To<TokenService>().AsSingle();
        container.Bind<IPaymentService>().To<PaymentService>().AsSingle();
        container.BindInterfacesAndSelfTo<SignalRClient>().AsSingle();

        _servicesBound = true;
    }
}

public class LoginServiceFactory : IServiceFactory
{
    public void Bind(DiContainer container)
    {
        container.Bind<LoginViewModel>().AsSingle();
    }
}

public class MainLobbyServiceFactory : IServiceFactory
{
    public void Bind(DiContainer container)
    {
        container.Bind<MainLobbyViewModel>().AsSingle();
        container.Bind<DeckViewModel>().AsSingle();
        container.Bind<CollectionViewModel>().AsSingle();
        container.Bind<CraftingViewModel>().AsSingle();
        container.Bind<ShopViewModel>().AsSingle();
        container.Bind<TutorialViewModel>().AsSingle();
    }
}

public class MatchMakingServiceFactory : IServiceFactory
{
    public void Bind(DiContainer container)
    {
        container.Bind<MatchMakingViewModel>().AsSingle();
        container.Bind<DeckViewModel>().AsSingle();
    }
}

public class FriendlyMatchServiceFactory : IServiceFactory
{
    public void Bind(DiContainer container)
    {
        container.Bind<MainLobbyViewModel>().AsSingle();
        container.Bind<FriendlyMatchViewModel>().AsSingle();
        container.Bind<MatchMakingViewModel>().AsSingle();
        container.Bind<DeckViewModel>().AsSingle();
    }
}

public class SinglePlayServiceFactory : IServiceFactory
{
    public void Bind(DiContainer container)
    {
        container.Bind<DeckViewModel>().AsSingle();
        container.Bind<SinglePlayViewModel>().AsSingle();
    }
}

public class GameServiceFactory : IServiceFactory
{
    public void Bind(DiContainer container)
    {
        container.Bind<GameViewModel>().AsSingle();
        container.Bind<TutorialViewModel>().AsSingle();
    }
}
