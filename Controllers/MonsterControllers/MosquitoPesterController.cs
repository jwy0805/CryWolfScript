using Google.Protobuf.Protocol;

public class MosquitoPesterController : MosquitoBugController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.MosquitoPester;
    }
}
