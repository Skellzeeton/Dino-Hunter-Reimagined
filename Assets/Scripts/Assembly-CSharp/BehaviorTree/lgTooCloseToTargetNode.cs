using BehaviorTree;
using UnityEngine;

public class lgTooCloseToTargetNode : Node
{
    public override Task CreateTask()
    {
        return new lgTooCloseToTargetTask(this);
    }
}

public class lgTooCloseToTargetTask : Task
{
    private const float MinAttackDistance = 1.0f;

    public lgTooCloseToTargetTask(Node node) : base(node) { }

    public override kTreeRunStatus OnUpdate(Object inputParam, float deltaTime)
    {
        CCharMob mob = inputParam as CCharMob;
        if (mob == null || mob.m_Target == null)
            return kTreeRunStatus.Failture;
        if (!(mob.m_Target is CCharPlayer) && !(mob.m_Target is CCharUser))
            return kTreeRunStatus.Failture;
        float dist = Vector3.Distance(mob.Pos, mob.m_Target.Pos);
        return (dist < MinAttackDistance) ? kTreeRunStatus.Success : kTreeRunStatus.Failture;
    }
}

public class doSetRepositionPathNode : Node
{
    public override Task CreateTask()
    {
        return new doSetRepositionPathTask(this);
    }
}

public class doSetRepositionPathTask : Task
{
    private const float RepositionDistance = 1.5f;
    private const float NavMeshSampleRadius = 3.0f;

    public doSetRepositionPathTask(Node node) : base(node) { }

    public override void OnEnter(Object inputParam)
    {
        CCharMob mob = inputParam as CCharMob;
        if (mob == null || mob.m_Target == null)
            return;
        if (!(mob.m_Target is CCharPlayer) && !(mob.m_Target is CCharUser))
            return;
        Vector3 dir = (mob.Pos - mob.m_Target.Pos).normalized;
        if (dir.sqrMagnitude < 0.001f)
            dir = Random.onUnitSphere;
        dir.y = 0f;
        Vector3 targetPos = mob.m_Target.Pos + dir * RepositionDistance;
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(targetPos, out hit, NavMeshSampleRadius, UnityEngine.AI.NavMesh.AllAreas))
        {
            mob.MoveTo(hit.position);
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                Vector3 randomDir = Random.insideUnitSphere;
                randomDir.y = 0f;
                randomDir.Normalize();
                targetPos = mob.m_Target.Pos + randomDir * RepositionDistance;
                if (UnityEngine.AI.NavMesh.SamplePosition(targetPos, out hit, NavMeshSampleRadius, UnityEngine.AI.NavMesh.AllAreas))
                {
                    mob.MoveTo(hit.position);
                    break;
                }
            }
        }
    }

    public override kTreeRunStatus OnUpdate(Object inputParam, float deltaTime)
    {
        return kTreeRunStatus.Success;
    }
}