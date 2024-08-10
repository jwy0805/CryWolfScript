using UnityEngine;
using Zenject;

public class ServiceInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IUserService>().To<UserService>().AsSingle();
        Container.Bind<IWebService>().To<WebService>().AsSingle();
        Container.Bind<ITokenService>().To<TokenService>().AsSingle();

        Container.Bind<LoginViewModel>().AsTransient();
    }
}