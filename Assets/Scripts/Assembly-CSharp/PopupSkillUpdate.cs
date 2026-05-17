using UnityEngine;

public class PopupSkillUpdate : MonoBehaviour
{
	public TUILabel label_title;

	public TUILabel label_introduce;

	public PopupSkillUpdateBuy btn_buy;

	public PopupWeaponUpdateSaleBuy btn_salebuy;

	public TUIMeshSprite img_sale_sign;

	public TUILabel label_sale_sign;
	
	public LevelStars level_stars;

	public void ShowSkillUpdate()
	{
		base.gameObject.transform.localPosition = new Vector3(0f, 0f, base.gameObject.transform.localPosition.z);
		base.gameObject.GetComponent<Animation>().Play();
	}

	public void HideSkillUpdate()
	{
		base.gameObject.transform.localPosition = new Vector3(0f, 1000f, base.gameObject.transform.localPosition.z);
	}

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
		int skillLevelMax = m_item.GetSkillLevelMax();
		int skillLevel = m_item.GetSkillLevel();
		label_introduce.Text = m_item.GetSkillIntroduce();
		label_title.Text = m_item.GetSkillName();
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
		TUIPriceInfo skillUpdatePrice = m_item.GetSkillUpdatePrice();
		if (skillUpdatePrice == null)
		{
			Debug.LogWarning("error!");
			return;
		}
		int price = skillUpdatePrice.price;
		UnitType unit_type = skillUpdatePrice.unit_type;
		if (flag && skillUnlock)
		{
			int now_price = Mathf.CeilToInt(discount * (float)skillUpdatePrice.price);
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
}