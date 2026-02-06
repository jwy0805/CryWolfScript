using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller<ProjectInstaller>
{
    public override void InstallBindings()
    {
        Container.Bind<IUserService>().To<UserService>().AsSingle();
        Container.Bind<IWebService>().To<WebService>().AsSingle();
        Container.Bind<ITokenService>().To<TokenService>().AsSingle();
        Container.Bind<IPaymentService>().To<PaymentService>().AsSingle();
        Container.Bind<ISecretService>().To<SecretService>().AsSingle();
        Container.BindInterfacesAndSelfTo<SignalRClient>().AsSingle();
    }
}
