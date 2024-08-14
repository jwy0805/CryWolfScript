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
    private static bool _servicesBound = false;
    
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
        }
    }

    public void CreateFactoryOnProjectContext(DiContainer container)
    {
        if (_servicesBound) return;
        
        container.Bind<IUserService>().To<UserService>().AsSingle();
        container.Bind<IWebService>().To<WebService>().AsSingle();
        container.Bind<ITokenService>().To<TokenService>().AsSingle();

        _servicesBound = true;
    }
}

public class LoginServiceFactory : IServiceFactory
{
    public void Bind(DiContainer container)
    {
        container.Bind<LoginViewModel>().AsTransient();
    }
}

public class MainLobbyServiceFactory : IServiceFactory
{
    public void Bind(DiContainer container)
    {
        container.Bind<MainLobbyViewModel>().AsTransient();
        container.Bind<DeckViewModel>().AsTransient();
        container.Bind<CollectionViewModel>().AsTransient();
    }
}
