using BehaviorTree;
using UnityEngine;

public class doRoarTask : Task
{
	protected float m_fTime;

	protected float m_fTimeCount;

	public doRoarTask(Node node)
		: base(node)
	{
	}

	public override void OnEnter(Object inputParam)
	{
		CCharMob cCharMob = inputParam as CCharMob;
		if (!(cCharMob == null))
		{
			m_fTime = cCharMob.CrossAnim(kAnimEnum.Mob_Roar, WrapMode.ClampForever, 0.3f, 1f, 0f);
			m_fTimeCount = 0f;
		}
	}

	public override void OnExit(Object inputParam)
	{
		base.OnExit(inputParam);
	}

	public override kTreeRunStatus OnUpdate(Object inputParam, float deltaTime)
	{
		CCharMob cCharMob = inputParam as CCharMob;
		if (cCharMob == null)
		{
			return kTreeRunStatus.Failture;
		}
		if (cCharMob.m_Target != null)
		{
			Vector3 dir = cCharMob.m_Target.Pos - cCharMob.Pos;
			dir.y = 0f;
			if (dir.sqrMagnitude > 0.0001f)
			{
				dir.Normalize();
				Vector3 newDir = Vector3.Lerp(
					cCharMob.Dir2D,
					dir,
					Mathf.Clamp01(m_fTimeCount / Mathf.Max(m_fTime, 0.0001f))
				);
				cCharMob.Dir2D = newDir;
			}
		}
		m_fTimeCount += deltaTime;
		if (m_fTimeCount < m_fTime)
		{
			return kTreeRunStatus.Executing;
		}
		return kTreeRunStatus.Success;
	}
}
