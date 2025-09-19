using System.Collections.Generic;
using UnityEngine;

public class PopupLevel_Frame03 : MonoBehaviour
{
	public GoodsNeedItemImg goods01;

	public GoodsNeedItemImg goods02;

	public GoodsNeedItemImg goods03;
	
	public GoodsNeedItemImg goods04;

	public GoodsNeedItemImg goods05;
	
	public GoodsNeedItemImg goods06;
	
	public GoodsNeedItemImg goods07;
	
	public GoodsNeedItemImg goods08;


	public PopupLevel_Recommend recommend;

	private Vector3 goods01_position = Vector3.zero;

	private Vector3 goods02_position = Vector3.zero;

	private Vector3 goods03_position = Vector3.zero;
	
	private Vector3 goods04_position = Vector3.zero;
	
	private Vector3 goods05_position = Vector3.zero;
	
	private Vector3 goods06_position = Vector3.zero;
	
	private Vector3 goods07_position = Vector3.zero;
	
	private Vector3 goods08_position = Vector3.zero;

	private void Awake()
	{
		if (goods01 == null || goods02 == null || goods03 == null || goods04 == null || goods05 == null || goods06 == null || goods07 == null || goods08 == null)
		{
			Debug.Log("error!");
		}
		goods01_position = goods01.transform.localPosition;
		goods02_position = goods02.transform.localPosition;
		goods03_position = goods03.transform.localPosition;
		goods04_position = goods04.transform.localPosition;
		goods05_position = goods05.transform.localPosition;
		goods06_position = goods06.transform.localPosition;
		goods07_position = goods07.transform.localPosition;
		goods08_position = goods08.transform.localPosition;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetGoodsInfo(List<TUIGoodsInfo> m_goods_drop_list)
	{
		if (m_goods_drop_list == null || m_goods_drop_list.Count == 0)
		{
			goods01.gameObject.SetActiveRecursive(false);
			goods02.gameObject.SetActiveRecursive(false);
			goods03.gameObject.SetActiveRecursive(false);
			goods04.gameObject.SetActiveRecursive(false);
			goods05.gameObject.SetActiveRecursive(false);
			goods06.gameObject.SetActiveRecursive(false);
			goods07.gameObject.SetActiveRecursive(false);
			goods08.gameObject.SetActiveRecursive(false);
			return;
		}
		switch (m_goods_drop_list.Count)
		{
		case 1:
			goods06.SetInfo(m_goods_drop_list[0].id, m_goods_drop_list[0].quality, m_goods_drop_list[0].name);
			goods06.transform.localPosition = goods06_position;
			goods01.gameObject.SetActiveRecursive(false);
			goods02.gameObject.SetActiveRecursive(false);
			goods03.gameObject.SetActiveRecursive(false);
			goods04.gameObject.SetActiveRecursive(false);
			goods05.gameObject.SetActiveRecursive(false);
			goods06.gameObject.SetActiveRecursive(true);
			goods07.gameObject.SetActiveRecursive(false);
			goods08.gameObject.SetActiveRecursive(false);
			break;
		case 2:
			goods06.SetInfo(m_goods_drop_list[0].id, m_goods_drop_list[0].quality, m_goods_drop_list[0].name);
			goods07.SetInfo(m_goods_drop_list[1].id, m_goods_drop_list[1].quality, m_goods_drop_list[1].name);
			goods06.transform.localPosition = goods06_position + new Vector3(20f, 0f, 0f);
			goods07.transform.localPosition = goods07_position + new Vector3(20f, 0f, 0f);
			goods01.gameObject.SetActiveRecursive(false);
			goods02.gameObject.SetActiveRecursive(false);
			goods03.gameObject.SetActiveRecursive(false);
			goods04.gameObject.SetActiveRecursive(false);
			goods05.gameObject.SetActiveRecursive(false);
			goods06.gameObject.SetActiveRecursive(true);
			goods07.gameObject.SetActiveRecursive(true);
			goods08.gameObject.SetActiveRecursive(false);
			break;
		case 3:
			goods06.SetInfo(m_goods_drop_list[0].id, m_goods_drop_list[0].quality, m_goods_drop_list[0].name);
			goods07.SetInfo(m_goods_drop_list[1].id, m_goods_drop_list[1].quality, m_goods_drop_list[1].name);
			goods08.SetInfo(m_goods_drop_list[2].id, m_goods_drop_list[2].quality, m_goods_drop_list[2].name);
			goods06.transform.localPosition = goods06_position;
			goods07.transform.localPosition = goods07_position;
			goods08.transform.localPosition = goods08_position;
			goods01.gameObject.SetActiveRecursive(false);
			goods02.gameObject.SetActiveRecursive(false);
			goods03.gameObject.SetActiveRecursive(false);
			goods04.gameObject.SetActiveRecursive(false);
			goods05.gameObject.SetActiveRecursive(false);
			goods06.gameObject.SetActiveRecursive(true);
			goods07.gameObject.SetActiveRecursive(true);
			goods08.gameObject.SetActiveRecursive(true);
			break;
		case 4:
			goods01.SetInfo(m_goods_drop_list[0].id, m_goods_drop_list[0].quality, m_goods_drop_list[0].name);
			goods02.SetInfo(m_goods_drop_list[1].id, m_goods_drop_list[1].quality, m_goods_drop_list[1].name);
			goods03.SetInfo(m_goods_drop_list[2].id, m_goods_drop_list[2].quality, m_goods_drop_list[2].name);
			goods04.SetInfo(m_goods_drop_list[3].id, m_goods_drop_list[3].quality, m_goods_drop_list[2].name);
			goods01.transform.localPosition = goods01_position;
			goods02.transform.localPosition = goods02_position;
			goods03.transform.localPosition = goods03_position;
			goods04.transform.localPosition = goods04_position;
			goods01.gameObject.SetActiveRecursive(true);
			goods02.gameObject.SetActiveRecursive(true);
			goods03.gameObject.SetActiveRecursive(true);
			goods04.gameObject.SetActiveRecursive(true);
			goods05.gameObject.SetActiveRecursive(false);
			goods06.gameObject.SetActiveRecursive(false);
			goods07.gameObject.SetActiveRecursive(false);
			goods08.gameObject.SetActiveRecursive(false);
			break;
		case 5:
			goods01.SetInfo(m_goods_drop_list[0].id, m_goods_drop_list[0].quality, m_goods_drop_list[0].name);
			goods02.SetInfo(m_goods_drop_list[1].id, m_goods_drop_list[1].quality, m_goods_drop_list[1].name);
			goods03.SetInfo(m_goods_drop_list[2].id, m_goods_drop_list[2].quality, m_goods_drop_list[2].name);
			goods04.SetInfo(m_goods_drop_list[3].id, m_goods_drop_list[3].quality, m_goods_drop_list[2].name);
			goods05.SetInfo(m_goods_drop_list[4].id, m_goods_drop_list[4].quality, m_goods_drop_list[2].name);
			goods01.transform.localPosition = goods01_position;
			goods02.transform.localPosition = goods02_position;
			goods03.transform.localPosition = goods03_position;
			goods04.transform.localPosition = goods04_position;
			goods05.transform.localPosition = goods05_position;
			goods01.gameObject.SetActiveRecursive(true);
			goods02.gameObject.SetActiveRecursive(true);
			goods03.gameObject.SetActiveRecursive(true);
			goods04.gameObject.SetActiveRecursive(true);
			goods05.gameObject.SetActiveRecursive(true);
			goods06.gameObject.SetActiveRecursive(false);
			goods07.gameObject.SetActiveRecursive(false);
			goods08.gameObject.SetActiveRecursive(false);
			break;
		default:
			Debug.Log("error!");
			break;
		}
	}

	public void SetRecommend(TUIRecommendRoleInfo m_recommend_role, TUIRecommendWeaponInfo m_recommend_weapon)
	{
		if (m_recommend_role != null)
		{
			recommend.SetRecommendRole(m_recommend_role);
		}
		else if (m_recommend_weapon != null)
		{
			recommend.SetRecommendWeapon(m_recommend_weapon);
		}
		else
		{
			recommend.SetRecommendNone();
		}
	}

	public bool GetOpenStart()
	{
		return recommend.GetOpenStart();
	}
}

