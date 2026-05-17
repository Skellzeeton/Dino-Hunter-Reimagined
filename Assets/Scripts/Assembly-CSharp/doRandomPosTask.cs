using BehaviorTree;
using UnityEngine;

public class doRandomPosTask : Task
{
    protected iGameSceneBase m_GameScene;
    protected UnityEngine.AI.NavMeshPath m_Path;

    public doRandomPosTask(Node node)
        : base(node)
    {
        m_Path = new UnityEngine.AI.NavMeshPath();
    }

    public override void OnEnter(Object inputParam)
    {
        m_GameScene = iGameApp.GetInstance().m_GameScene;
    }

    public override kTreeRunStatus OnUpdate(Object inputParam, float deltaTime)
    {
        CCharMob cCharMob = inputParam as CCharMob;
        if (cCharMob == null)
        {
            return kTreeRunStatus.Failture;
        }
		if (cCharMob.m_bIsOffNavMesh || cCharMob.m_bRecoveryActive)
        {
            return kTreeRunStatus.Failture;
        }
        Vector3 startPos = cCharMob.Pos;
        UnityEngine.AI.NavMeshHit startHit;
        if (!UnityEngine.AI.NavMesh.SamplePosition(startPos, out startHit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            cCharMob.m_bIsOffNavMesh = true;
            return kTreeRunStatus.Failture;
        }
        Vector3 targetPosition = startHit.position + new Vector3(Random.Range(-30f, 30f), 0f, Random.Range(-30f, 30f));
        cCharMob.m_ltPath.Clear();
        if (!UnityEngine.AI.NavMesh.CalculatePath(startHit.position, targetPosition, -1, m_Path))
        {
            return kTreeRunStatus.Failture;
        }
        for (int i = 0; i < m_Path.corners.Length; i++)
        {
            cCharMob.m_ltPath.Add(m_Path.corners[i]);
        }
        return kTreeRunStatus.Success;
    }
}