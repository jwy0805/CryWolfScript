using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;

public class MonsterController : CreatureController
{
    protected MyPlayerController PlayerController;
    
    protected override void Init()
    {
        base.Init();
        ObjectType = GameObjectType.Monster;
        transform.rotation = Quaternion.Euler(0, PosInfo.Dir, 0);
    }
    
    protected override void UpdateMoving()
    {
        if (PathQueue.Count <= 1) return;
        // Path 설정
        Vector3 cellPos = PathQueue.Peek();
        float distance = Vector3.Distance(transform.position, cellPos);
        if (distance < Stat.AttackRange) return;
        float step = TotalMoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, cellPos, step);
        // Dir 설정
        double moveDir = DirQueue.Peek();
        Quaternion quaternion = Quaternion.Euler(0, (float)moveDir, step);
        transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, step);
        // Queue 정리
        if (distance > 0.05f) return;
        DirQueue.Dequeue();
        PathQueue.Dequeue();
    }

    protected override void UpdateAttack()
    {
        base.UpdateAttack();
        if (SyncPos == new Vector3(0, 0, 0)) return;
        if (Vector3.Distance(transform.position, SyncPos) < 0.05f)
        {
            SyncPos = new Vector3(0, 0, 0);
            return;
        }
        
        float step = 2 * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, SyncPos, step);
    }

    protected override void UpdateSkill()
    {
        UpdateAttack();
    }
}
