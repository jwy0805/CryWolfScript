using Zenject;

public static class ServiceResolver
{
    public static IUserService UserService => ProjectContext.Instance.Container.Resolve<IUserService>();
    public static IWebService WebService => ProjectContext.Instance.Container.Resolve<IWebService>();
    
    public static TutorialViewModel ResolveTutorialViewModel()
    {
        var sceneContext = UnityEngine.Object.FindAnyObjectByType<SceneContext>();
        return sceneContext != null ? sceneContext.Container.Resolve<TutorialViewModel>() : null;
    }
}
