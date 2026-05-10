using System.Collections.Generic;
using UnityEngine;

public class PopupLevel : MonoBehaviour
{
	public GameObject go_popup;
	public PopupLevel_Frame01 popuplevel_frame01;
	public PopupLevel_Frame02 popuplevel_frame02;
	public PopupLevel_Frame03 popuplevel_frame03;
	public TUILabel label_title;
	public TUIMeshSprite img_title_bg;
	public TUIButtonClick btn_start;
	public PopupTips popup_tips;
	public List<PopupLevel_Item> popup_level_item_list;
	private TUIMainLevelInfo level_info;
	private PopupLevel_Item  level_item_now;

	public void SetBtnStartEnable(bool m_enable)
	{
		if (btn_start == null) return;
		btn_start.Disable(!m_enable);
	}

	public void SetInfo(TUIMainLevelInfo m_info)
	{
		level_info = m_info;
		if (level_info == null) { Debug.LogWarning("[PopupLevel] SetInfo: null"); return; }
		if (popup_level_item_list == null) { Debug.LogWarning("[PopupLevel] SetInfo: no items list"); return; }

		List<TUISecondaryLevelInfo> secondary_level_info = m_info.secondary_level_info;
		int secondary_level_id   = m_info.secondary_level_id;
		int[] level_goods_drop_list = m_info.level_goods_drop_list;
		int count = popup_level_item_list.Count;
		if (label_title != null)    label_title.Text = m_info.title;
		if (img_title_bg != null)   img_title_bg.texture = TUIMappingInfo.Instance().GetMapTexture((int)m_info.level_type);
		if (secondary_level_info == null || secondary_level_info.Count != count)
		{
			Debug.LogWarning("[PopupLevel] SetInfo: count mismatch info=" +
			          (secondary_level_info == null ? "null" : secondary_level_info.Count.ToString()) +
			          " items=" + count);
			return;
		}
		int choose = 0;
		for (int i = 0; i < count; i++)
		{
			PopupLevel_Item item = popup_level_item_list[i];
			if (item == null) continue;
			TUISecondaryLevelInfo info = secondary_level_info[i];
			if (info == null) continue;
			item.SetInfo(info, info.pass_state);
			if (info.id == secondary_level_id) choose = i;
			if (level_goods_drop_list != null)
				foreach (int gid in level_goods_drop_list)
					if (info.id == gid) item.ShowDropSign(true);
		}
		SetChoose(choose);
	}

	public void Show()
	{
		base.transform.localPosition = new Vector3(0f, 0f, base.transform.localPosition.z);
		if (go_popup != null && go_popup.GetComponent<Animation>() != null)
			go_popup.GetComponent<Animation>().Play();
	}

	public void Hide()
	{
		base.transform.localPosition = new Vector3(0f, -1000f, base.transform.localPosition.z);
		if (popup_level_item_list == null) return;
		foreach (PopupLevel_Item item in popup_level_item_list)
		{
			if (item == null) continue;
			item.ShowDropSign(false);
			TUIButtonSelect btn = item.GetBtnSelect();
			if (btn != null) btn.Reset();
		}
	}

	public void ShowTips(TUIControl m_control)
	{
		if (popup_tips == null || m_control == null) return;
		GoodsNeedItemImg component = m_control.GetComponent<GoodsNeedItemImg>();
		if (component != null)
			popup_tips.SetInfo(component.GetGoodsName(), m_control.transform.position, PopupTips.TipsPivot.TopRight);
	}

	public void HideTips()
	{
		if (popup_tips != null) popup_tips.Hide();
	}
	
	public void SetChoose(int m_index)
	{
		if (m_index < 0)
		{
			ClearFrames();
			SetBtnStartEnable(false);
			return;
		}
		if (popup_level_item_list == null) return;
		level_item_now = popup_level_item_list[m_index];
		if (level_item_now == null) { ClearFrames(); SetBtnStartEnable(false); return; }
		TUISecondaryLevelInfo info = level_item_now.GetInfo();
		if (info == null) return;
		UpdateTitleIcon(info);
		LevelPassState state = level_item_now.GetState();
		if (state == LevelPassState.Disable)
		{
			ClearFrames();
			SetBtnStartEnable(false);
		}
		else
		{
			ApplyFrames(info);
			RefreshStartButton();
		}
		TUIButtonSelect btnSel = level_item_now.GetBtnSelect();
		if (btnSel != null) btnSel.SetSelected(true);
	}
	
	public void SetChoose(PopupLevel_Item m_control)
	{
		if (popup_level_item_list == null) return;
		level_item_now = m_control;
		if (level_item_now == null) { ClearFrames(); SetBtnStartEnable(false); return; }
		TUIButtonSelect btnSel = level_item_now.GetBtnSelect();
		TUISecondaryLevelInfo info  = level_item_now.GetInfo();
		UpdateTitleIcon(info);
		LevelPassState state = level_item_now.GetState();
		if (btnSel != null) btnSel.SetSelected(true);
		if (info == null) return;
		if (state == LevelPassState.Disable)
		{
			ClearFrames();
			SetBtnStartEnable(false);
			return;
		}
		ApplyFrames(info);
		RefreshStartButton();
	}

	private void UpdateTitleIcon(TUISecondaryLevelInfo info)
	{
		if (img_title_bg == null || info == null) return;
		iGameLevelCenter levelCenter = iGameApp.GetInstance().m_GameData.m_GameLevelCenter;
		GameLevelInfo levelInfo = levelCenter?.Get(info.id);
		if (levelInfo == null) return;
		string iconTex;
		if (!string.IsNullOrEmpty(levelInfo.sIconOverride))
		{
			if (int.TryParse(levelInfo.sIconOverride, out int overrideId))
			{
				iconTex = TUIMappingInfo.Instance().GetMapTexture(overrideId);
			}
			else
			{
				iconTex = levelInfo.sIconOverride;
			}
		}
		else if (!string.IsNullOrEmpty(levelInfo.sIcon))
		{
			if (int.TryParse(levelInfo.sIcon, out int iconId))
			{
				iconTex = TUIMappingInfo.Instance().GetMapTexture(iconId);
			}
			else
			{
				iconTex = levelInfo.sIcon;
			}
		}
		else if (level_info != null)
		{
			iconTex = TUIMappingInfo.Instance().GetMapTexture((int)level_info.level_type);
		}
		else
		{
			return;
		}
		img_title_bg.texture = iconTex;
	}
	
	public PopupLevel_Item GetChoose() { return level_item_now; }
	
	private void ClearFrames()
	{
		if (popuplevel_frame01 != null) popuplevel_frame01.SetInfo(string.Empty);
		if (popuplevel_frame02 != null) popuplevel_frame02.SetInfo(string.Empty);
		if (popuplevel_frame03 != null) { popuplevel_frame03.SetGoodsInfo(null); popuplevel_frame03.SetRecommend(null, null); }
	}

	private void ApplyFrames(TUISecondaryLevelInfo info)
	{
		if (popuplevel_frame01 != null) popuplevel_frame01.SetInfo(info.introduce01);
		if (popuplevel_frame02 != null) popuplevel_frame02.SetInfo(info.introduce02);
		if (popuplevel_frame03 != null)
		{
			popuplevel_frame03.SetGoodsInfo(info.goods_drop_list);
			popuplevel_frame03.SetRecommend(info.recommend_role_info, info.recommend_weapon_info);
		}
	}

	private void RefreshStartButton()
	{
		bool canStart = level_item_now != null &&
		                level_item_now.GetState() != LevelPassState.Disable;
		if (canStart && popuplevel_frame03 != null && !popuplevel_frame03.GetOpenStart())
			canStart = false;
		SetBtnStartEnable(canStart);
	}
}