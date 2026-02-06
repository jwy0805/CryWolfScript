using UnityEngine;
using Zenject;

public class LoginInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<LoginViewModel>().AsSingle();
    }
}
