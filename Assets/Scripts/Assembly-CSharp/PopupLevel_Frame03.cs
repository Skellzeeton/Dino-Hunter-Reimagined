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
	public PopupLevel_Frame02 frame02;
	public GameObject btn_toggle_arrow;
	private Vector3 goods01_position = Vector3.zero;
	private Vector3 goods02_position = Vector3.zero;
	private Vector3 goods03_position = Vector3.zero;
	private Vector3 goods04_position = Vector3.zero;
	private Vector3 goods05_position = Vector3.zero;
	private Vector3 goods06_position = Vector3.zero;
	private Vector3 goods07_position = Vector3.zero;
	private Vector3 goods08_position = Vector3.zero;
	private TUIRecommendRoleInfo   m_cached_role_info;
	private TUIRecommendWeaponInfo m_cached_weapon_info;
	private List<TUIGoodsInfo>     m_cached_goods_list;
	private bool m_showing_recommend = true;

	private void Awake()
	{
		if (goods01 == null || goods02 == null || goods03 == null || goods04 == null ||
		    goods05 == null || goods06 == null || goods07 == null || goods08 == null)
			Debug.LogWarning("error!");
		goods01_position = goods01.transform.localPosition;
		goods02_position = goods02.transform.localPosition;
		goods03_position = goods03.transform.localPosition;
		goods04_position = goods04.transform.localPosition;
		goods05_position = goods05.transform.localPosition;
		goods06_position = goods06.transform.localPosition;
		goods07_position = goods07.transform.localPosition;
		goods08_position = goods08.transform.localPosition;
	}

	public void SetGoodsInfo(List<TUIGoodsInfo> m_goods_drop_list)
	{
		m_cached_goods_list = m_goods_drop_list;
		ApplyGoodsInfo(m_goods_drop_list);
	}

	public void SetRecommend(TUIRecommendRoleInfo m_recommend_role,
		TUIRecommendWeaponInfo m_recommend_weapon)
	{
		m_cached_role_info   = m_recommend_role;
		m_cached_weapon_info = m_recommend_weapon;
		recommend.gameObject.SetActive(true);
		ApplyRecommendLogic(m_recommend_role, m_recommend_weapon);
		if (!m_showing_recommend)
		{
			recommend.gameObject.SetActive(false);
			ApplyFrame02Visibility(true);
		}
		else
		{
			ApplyFrame02Visibility(false);
		}
		ApplyArrowRotation();
	}

	public bool GetOpenStart()
	{
		return recommend.GetOpenStart();
	}

	public void ToggleRecommendPanel()
	{
		m_showing_recommend = !m_showing_recommend;

		if (m_showing_recommend)
		{
			recommend.gameObject.SetActive(true);
			ApplyFrame02Visibility(false);
		}
		else
		{
			recommend.gameObject.SetActive(false);
			ApplyFrame02Visibility(true);
		}

		ApplyArrowRotation();
	}

	private void ApplyRecommendLogic(TUIRecommendRoleInfo roleInfo,
	                                  TUIRecommendWeaponInfo weaponInfo)
	{
		if (roleInfo != null)
			recommend.SetRecommendRole(roleInfo);
		else if (weaponInfo != null)
		{
			if (IsWeaponID(weaponInfo.id))
				recommend.SetRecommendWeapon(weaponInfo);
			else
				recommend.SetRecommendAvatarItem(weaponInfo);
		}
		else
			recommend.SetRecommendNone();
	}

	private void ApplyFrame02Visibility(bool show)
	{
		if (frame02 != null)
			frame02.gameObject.SetActiveRecursive(show);
	}

	private void ApplyArrowRotation()
	{
		if (btn_toggle_arrow == null) return;
		float yAngle = m_showing_recommend ? 180f : 0f;
		btn_toggle_arrow.transform.localRotation = Quaternion.Euler(0f, yAngle, 0f);
	}

	private void ApplyGoodsInfo(List<TUIGoodsInfo> m_goods_drop_list)
	{
		HideAllGoods();
		if (m_goods_drop_list == null || m_goods_drop_list.Count == 0)
			return;
		switch (m_goods_drop_list.Count)
		{
		case 1:
			goods06.SetInfo(m_goods_drop_list[0].id, m_goods_drop_list[0].quality, m_goods_drop_list[0].name);
			goods06.transform.localPosition = goods06_position;
			goods06.gameObject.SetActiveRecursive(true);
			break;
		case 2:
			goods06.SetInfo(m_goods_drop_list[0].id, m_goods_drop_list[0].quality, m_goods_drop_list[0].name);
			goods07.SetInfo(m_goods_drop_list[1].id, m_goods_drop_list[1].quality, m_goods_drop_list[1].name);
			goods06.transform.localPosition = goods06_position + new Vector3(20f, 0f, 0f);
			goods07.transform.localPosition = goods07_position + new Vector3(20f, 0f, 0f);
			goods06.gameObject.SetActiveRecursive(true);
			goods07.gameObject.SetActiveRecursive(true);
			break;
		case 3:
			goods06.SetInfo(m_goods_drop_list[0].id, m_goods_drop_list[0].quality, m_goods_drop_list[0].name);
			goods07.SetInfo(m_goods_drop_list[1].id, m_goods_drop_list[1].quality, m_goods_drop_list[1].name);
			goods08.SetInfo(m_goods_drop_list[2].id, m_goods_drop_list[2].quality, m_goods_drop_list[2].name);
			goods06.transform.localPosition = goods06_position;
			goods07.transform.localPosition = goods07_position;
			goods08.transform.localPosition = goods08_position;
			goods06.gameObject.SetActiveRecursive(true);
			goods07.gameObject.SetActiveRecursive(true);
			goods08.gameObject.SetActiveRecursive(true);
			break;
		case 4:
			goods01.SetInfo(m_goods_drop_list[0].id, m_goods_drop_list[0].quality, m_goods_drop_list[0].name);
			goods02.SetInfo(m_goods_drop_list[1].id, m_goods_drop_list[1].quality, m_goods_drop_list[1].name);
			goods03.SetInfo(m_goods_drop_list[2].id, m_goods_drop_list[2].quality, m_goods_drop_list[2].name);
			goods04.SetInfo(m_goods_drop_list[3].id, m_goods_drop_list[3].quality, m_goods_drop_list[3].name);
			goods01.transform.localPosition = goods01_position;
			goods02.transform.localPosition = goods02_position;
			goods03.transform.localPosition = goods03_position;
			goods04.transform.localPosition = goods04_position;
			goods01.gameObject.SetActiveRecursive(true);
			goods02.gameObject.SetActiveRecursive(true);
			goods03.gameObject.SetActiveRecursive(true);
			goods04.gameObject.SetActiveRecursive(true);
			break;
		case 5:
			goods01.SetInfo(m_goods_drop_list[0].id, m_goods_drop_list[0].quality, m_goods_drop_list[0].name);
			goods02.SetInfo(m_goods_drop_list[1].id, m_goods_drop_list[1].quality, m_goods_drop_list[1].name);
			goods03.SetInfo(m_goods_drop_list[2].id, m_goods_drop_list[2].quality, m_goods_drop_list[2].name);
			goods04.SetInfo(m_goods_drop_list[3].id, m_goods_drop_list[3].quality, m_goods_drop_list[3].name);
			goods05.SetInfo(m_goods_drop_list[4].id, m_goods_drop_list[4].quality, m_goods_drop_list[4].name);
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
			break;
		default:
			Debug.LogWarning("error!");
			break;
		}
	}

	private void HideAllGoods()
	{
		ClearGoods(goods01);
		ClearGoods(goods02);
		ClearGoods(goods03);
		ClearGoods(goods04);
		ClearGoods(goods05);
		ClearGoods(goods06);
		ClearGoods(goods07);
		ClearGoods(goods08);
	}

	private void ClearGoods(GoodsNeedItemImg item)
	{
		if (item == null) return;
		item.SetInfo(0, GoodsQualityType.Quality01, string.Empty);
		item.gameObject.SetActiveRecursive(false);
	}

	private bool IsWeaponID(int id)
	{
		iGameData gameData = iGameApp.GetInstance().m_GameData;
		if (gameData == null) return false;
		iWeaponCenter weaponCenter = gameData.GetWeaponCenter();
		if (weaponCenter == null) return false;
		return weaponCenter.Get(id) != null;
	}
}