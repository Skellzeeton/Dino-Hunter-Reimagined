using BehaviorTree;
using UnityEngine;
using System.Collections.Generic;

public class doSelectTargetBuildingTask : Task
{
    protected iGameSceneBase m_GameScene;
    protected iGameData m_GameData;
    protected UnityEngine.AI.NavMeshPath m_NavPath;

    public doSelectTargetBuildingTask(Node node)
        : base(node)
    {
        m_NavPath = new UnityEngine.AI.NavMeshPath();
    }

    public override void OnEnter(Object inputParam)
    {
        m_GameScene = iGameApp.GetInstance().m_GameScene;
        m_GameData = iGameApp.GetInstance().m_GameData;
    }

    public override kTreeRunStatus OnUpdate(Object inputParam, float deltaTime)
    {
        CCharMob cCharMob = inputParam as CCharMob;
        if (cCharMob == null) return kTreeRunStatus.Failture;

        CMobInfoLevel mobInfo = m_GameData.GetMobInfo(cCharMob.ID, cCharMob.Level);
        if (mobInfo == null) return kTreeRunStatus.Failture;

        iBuilding curBuilding = m_GameScene.CurBuilding;
        if (curBuilding == null) return kTreeRunStatus.Failture;

        Vector3 mobPos = cCharMob.GetBone(2).position;
        Vector3 targetPoint = curBuilding.GetRandomPoint(); // fallback

        // ✅ Lock attack points only once
        if (cCharMob.m_LockedAttackPoints == null || cCharMob.m_LockedAttackPoints.Count == 0)
        {
            List<Vector3> attackPoints = curBuilding.GetAttackPoints();
            if (attackPoints != null && attackPoints.Count > 0)
            {
                attackPoints.Sort((a, b) =>
                    Vector3.SqrMagnitude(a - mobPos).CompareTo(Vector3.SqrMagnitude(b - mobPos)));

                int count = Mathf.Min(5, attackPoints.Count);
                cCharMob.m_LockedAttackPoints = attackPoints.GetRange(0, count);
            }
        }

        // ✅ Pick from locked points only
        if (cCharMob.m_LockedAttackPoints != null && cCharMob.m_LockedAttackPoints.Count > 0)
        {
            targetPoint = cCharMob.m_LockedAttackPoints[Random.Range(0, cCharMob.m_LockedAttackPoints.Count)];
        }

        Vector3 normalized = (targetPoint - mobPos).normalized;
        normalized.y = 0f;

        cCharMob.m_TargetBuilding = curBuilding;
        if (cCharMob.m_TargetBuilding == null || cCharMob.m_TargetBuilding.IsBroken)
        {
            return kTreeRunStatus.Failture;
        }

        Vector3 vector = targetPoint;
        vector.y = curBuilding.transform.position.y;

        if (!UnityEngine.AI.NavMesh.CalculatePath(cCharMob.Pos, vector, -1, m_NavPath))
        {
            return kTreeRunStatus.Failture;
        }

        cCharMob.m_ltPath.Clear();
        for (int i = 0; i < m_NavPath.corners.Length; i++)
        {
            cCharMob.m_ltPath.Add(m_NavPath.corners[i]);
        }

        if (cCharMob.m_ltPath.Count < 2)
        {
            cCharMob.m_ltPath[0] = cCharMob.m_ltPath[0] - normalized * mobInfo.fMeleeRange;
        }
        else
        {
            int index = cCharMob.m_ltPath.Count - 1;
            if (Vector3.Distance(cCharMob.m_ltPath[index], vector) < mobInfo.fMeleeRange)
            {
                cCharMob.m_ltPath[index] = vector - normalized * mobInfo.fMeleeRange;
            }
        }

        return kTreeRunStatus.Success;
    }
}
