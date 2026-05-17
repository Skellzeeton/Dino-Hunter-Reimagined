using BehaviorTree;
using UnityEngine;

public class doRecoverToNavMeshTask : Task
{
    protected iGameSceneBase m_GameScene;
    private const float RecoveryMoveSpeed = 5.0f;

    public doRecoverToNavMeshTask(Node node)
        : base(node)
    {
    }

    public override void OnEnter(Object inputParam)
    {
        m_GameScene = iGameApp.GetInstance().m_GameScene;
        CCharMob cCharMob = inputParam as CCharMob;
        if (cCharMob != null)
        {
            cCharMob.SetCurTask(this);
        }
    }

    public override kTreeRunStatus OnUpdate(Object inputParam, float deltaTime)
    {
        CCharMob cCharMob = inputParam as CCharMob;
        if (cCharMob == null)
        {
            return kTreeRunStatus.Failture;
        }
        if (!cCharMob.m_bIsOffNavMesh && !cCharMob.m_bRecoveryActive)
        {
            return kTreeRunStatus.Success;
        }
        if (cCharMob.m_ltPath.Count == 0)
        {
            if (!cCharMob.RecoverToNavMesh())
            {
                return kTreeRunStatus.Failture;
            }
        }
        if (cCharMob.m_ltPath.Count > 0)
        {
            Vector3 targetPoint = cCharMob.m_ltPath[0];
            Vector3 direction = (targetPoint - cCharMob.Pos).normalized;
            direction.y = 0f;
            float moveSpeed = RecoveryMoveSpeed;
            float moveDistance = moveSpeed * deltaTime;
            Vector3 newPos = cCharMob.Pos + direction * moveDistance;
            if (Vector3.Distance(cCharMob.Pos, targetPoint) <= moveDistance * 1.5f)
            {
                cCharMob.m_ltPath.RemoveAt(0);
                if (cCharMob.m_ltPath.Count == 0)
                {
                    cCharMob.m_bIsOffNavMesh = false;
                    cCharMob.m_bRecoveryActive = false;
                    cCharMob.m_bHasPurposePoint = false;
                    return kTreeRunStatus.Success;
                }
            }
            else
            {
                cCharMob.transform.position = newPos;
            }
            return kTreeRunStatus.Executing;
        }
        return kTreeRunStatus.Failture;
    }
}