using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class CoroutineManager
{
    private CoroutineRunner _runner;

    public void Init()
    {
        _runner = Managers.Instance.GetOrAddComponent<CoroutineRunner>();
    }

    public Task WaitUntilNextFrameAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        _runner.StartCoroutine(WaitUntilNextFrame(tcs));
        return tcs.Task;
        
        static IEnumerator WaitUntilNextFrame(TaskCompletionSource<bool> tcs)
        {
            yield return null;
            tcs.TrySetResult(true);
        }
    }
}
