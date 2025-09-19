using UnityEngine;

[RequireComponent(typeof(TAudioController))]
public class iDinoHunterAudioController : MonoBehaviour
{
	public bool m_bBoss;
	
	public bool m_bLava;
	
	public bool m_bIce;
	
	public bool m_bZombie;

	protected TAudioController m_AudioController;

	private void Awake()
	{
		m_AudioController = GetComponent<TAudioController>();
	}

	public void PlayAudioByMobType(string sName)
	{
		if (!(m_AudioController == null) && !(base.transform.root == null))
		{
			if (m_bBoss)
			{
				sName += "_Boss";
			}
			if (m_bLava)
			{
				sName += "_Lava";
			}
			if (m_bIce)
			{
				sName += "_Ice";
			}
			if (m_bZombie)
			{
				sName += "_Zombie";
			}
			m_AudioController.PlayAudio(sName);
		}
	}
}
