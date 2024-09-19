using Google.Protobuf.Protocol;
using UnityEngine;
using State = Google.Protobuf.Protocol.State;

public class SheepController : CreatureController
{
    protected override void Init()
    {
        base.Init();
        ObjectType = GameObjectType.Sheep;
    }
    
    protected override void UpdateMoving()
    {
        if (PathQueue.Count <= 1) return;
        
        // Path 설정
        Vector3 cellPos = PathQueue.Peek();
        float step = TotalMoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, cellPos, step);
        
        // Dir 설정
        double moveDir = DirQueue.Peek();
        Quaternion quaternion = Quaternion.Euler(0, (float)moveDir, step);
        transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, step);
        
        // Queue 정리
        if (Vector3.Distance(transform.position, cellPos) > 0.1f) return;
        DirQueue.Dequeue();
        PathQueue.Dequeue();
    }
}