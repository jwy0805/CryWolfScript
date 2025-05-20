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
        DrawPath();
        
        // Path 설정
        Vector3 cellPos = PathQueue.Peek();
        float distance = Vector3.Distance(transform.position, cellPos);
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

    // Test - Draw path
    private void DrawPath()
    {
        // 1) 큐를 배열로 복사
        var points = PathQueue.ToArray();
        float yOffset = 0.1f;  // 지면 위로 띄울 높이
        float crossSize = 0.05f;

        // 2) 인접한 점들만 연결
        for (int i = 1; i < points.Length; i++)
        {
            var prev = points[i - 1] + Vector3.up * yOffset;
            var curr = points[i]     + Vector3.up * yOffset;

            // 선분
            Debug.DrawLine(prev, curr, Color.green);

            // 십자 표시 (크로스)
            Debug.DrawRay(curr + Vector3.left  * crossSize, Vector3.right * crossSize, Color.green);
            Debug.DrawRay(curr + Vector3.down   * crossSize, Vector3.up    * crossSize, Color.green);
            
            // Debug.Log($"{prev.x}, {prev.y}, {prev.z} / {curr.x}, {curr.y}, {curr.z}");
        }
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
