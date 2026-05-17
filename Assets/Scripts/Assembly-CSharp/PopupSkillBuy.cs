using UnityEngine;

public class PopupSkillBuy : MonoBehaviour
{
	public TUILabel label_title;

	public TUILabel label_introduce;

	public PopupSkillUpdateBuy btn_buy;

	public GameObject go_popup;

	private string gold_texture = "title_jingbi";

	private string crystal_texture = "title_shuijing";

	public PopupWeaponUpdateSaleBuy btn_salebuy;

	public TUIMeshSprite img_sale_sign;

	public TUILabel label_sale_sign;
	
	public LevelStars level_stars;
	
	public void SetInfo(ScrollList_SkillItem m_item)
	{
		if (m_item == null)
		{
			Debug.LogWarning("error!");
			return;
		}
		bool skillUnlock = m_item.GetSkillUnlock();
		float discount = m_item.GetDiscount();
		bool flag = !(discount >= 1f);
		int skillLevel = m_item.GetSkillLevel();
		int skillLevelMax = m_item.GetSkillLevelMax();
		if (skillLevel >= skillLevelMax)
		{
			return;
		}
		TUIPriceInfo skillBuyPrice = m_item.GetSkillBuyPrice();
		if (skillBuyPrice == null)
		{
			Debug.LogWarning("error!");
			return;
		}
		int price = skillBuyPrice.price;
		UnitType unit_type = skillBuyPrice.unit_type;
		string skillIntroduceEx = m_item.GetSkillIntroduceEx();
		string skillName = m_item.GetSkillName();
		if (label_title != null)
		{
			label_title.Text = skillName;
			if (level_stars != null)
			{
				float x = label_title.CalculateBounds(label_title.Text).size.x;
				x *= label_title.transform.localScale.x;
				Vector3 position = new Vector3(label_title.transform.localPosition.x + x + 12f, label_title.transform.localPosition.y, label_title.transform.localPosition.z);
				if (skillLevelMax > 0)
				{
					int currentLevel = (skillLevel > 0) ? skillLevel : 0;
					level_stars.SetStars(currentLevel, skillLevelMax, position);
				}
				else
				{
					level_stars.SetStarsDisable();
				}
			}
		}
		
		if (label_introduce != null)
		{
			label_introduce.Text = skillIntroduceEx;
		}
		
		if (flag && skillUnlock)
		{
			int now_price = Mathf.CeilToInt(discount * (float)skillBuyPrice.price);
			if (btn_salebuy != null)
			{
				btn_salebuy.gameObject.SetActive(true);
				TUIButtonClick component = btn_salebuy.GetComponent<TUIButtonClick>();
				if (component != null)
				{
					component.Reset();
				}
				btn_salebuy.SetBtnText(price, unit_type, now_price, unit_type);
			}
			if (btn_buy != null)
			{
				btn_buy.gameObject.SetActive(false);
			}
			if (img_sale_sign != null)
			{
				img_sale_sign.gameObject.SetActive(true);
			}
			if (label_sale_sign != null)
			{
				label_sale_sign.gameObject.SetActive(true);
				label_sale_sign.Text = (int)((1f - discount) * 100f + 0.5f) + "% off";
			}
			return;
		}
		if (btn_salebuy != null)
		{
			btn_salebuy.gameObject.SetActive(false);
		}
		if (img_sale_sign != null)
		{
			img_sale_sign.gameObject.SetActive(false);
		}
		if (label_sale_sign != null)
		{
			label_sale_sign.gameObject.SetActive(false);
		}
		if (btn_buy != null)
		{
			btn_buy.gameObject.SetActive(true);
			TUIButtonClick component2 = btn_buy.GetComponent<TUIButtonClick>();
			if (component2 != null)
			{
				component2.Reset();
			}
			btn_buy.SetBtnText(price, unit_type);
		}
	}

	public void Show()
	{
		base.gameObject.transform.localPosition = new Vector3(0f, 0f, base.gameObject.transform.localPosition.z);
		if (go_popup != null && go_popup.GetComponent<Animation>() != null)
		{
			go_popup.GetComponent<Animation>().Play();
		}
	}

	public void Hide()
	{
		base.gameObject.transform.localPosition = new Vector3(0f, -1000f, base.gameObject.transform.localPosition.z);
	}
}