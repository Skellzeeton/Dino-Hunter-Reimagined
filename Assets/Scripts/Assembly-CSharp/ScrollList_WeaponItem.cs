using UnityEngine;

public class ScrollList_WeaponItem : MonoBehaviour
{
	public TUIMeshSprite img_bg;
	public TUIMeshSprite img_frame;
	public TUIMeshSprite img_frame_choose;
	public TUIMeshSprite img_new;
	public TUIMeshSprite img_lock;
	public TUIMeshSprite img_sale;
	public TUILabel label_sale;

	private string img_sale_sign_texture01 = "sale_onsalebiaoqian";
	private string img_sale_sign_texture02 = "sale_onsalebiaoqian2";
	private bool be_choose;
	private TUIWeaponAttributeInfo attribute_info;
	private string NGUI_weapon_texture_path = "Artist/Textrues/Weapon/";
	private string NGUI_weapon_altas_path = "Artist/Atlas/Weapon/";
	private string texture_mark = "new";
	private string texture_new = "new2";
	private string texture_frame_normal = "wuqikuang2";
	private string texture_frame_normal_choose = "wuqikuang";
	private string texture_frame_crystal = "wuqikuang2_2";
	private string texture_frame_crystal_choose = "wuqikuang_1";

	public NewMarkType m_MarkType { get; private set; }

	public void DoCreate(TUIWeaponAttributeInfo weaponattributeinfo)
	{
		if (weaponattributeinfo == null)
		{
			Debug.LogWarning("error!");
			return;
		}
		attribute_info = weaponattributeinfo;
		be_choose = true;
		if (img_bg != null)
		{
			string text = attribute_info.m_sIcon;
			if (text.Length < 1)
			{
				text = TUIMappingInfo.Instance().GetWeaponTexture(attribute_info.m_nID);
			}
			string path = string.Empty;
			if (attribute_info.IsWeapon())
			{
				path = TUIMappingInfo.Instance().m_sPathRootCustomTex + "/Weapon/" + text;
			}
			else if (attribute_info.IsArmor())
			{
				path = TUIMappingInfo.Instance().m_sPathRootCustomTex + "/Armor/" + text;
			}
			else if (attribute_info.IsAccessory())
			{
				path = TUIMappingInfo.Instance().m_sPathRootCustomTex + "/Accessory/" + text;
			}
			SetCustomizeTexture(img_bg, path, true);
		}
		OnUnselect();
		if (!attribute_info.m_bUnlock)
		{
			img_lock.gameObject.SetActiveRecursive(true);
			// Hide new/mark indicators for locked items
			if (img_new != null)
			{
				img_new.texture = string.Empty;
				img_new.gameObject.SetActiveRecursive(false);
			}
		}
		else
		{
			img_lock.gameObject.SetActiveRecursive(false);
			// Only show new/mark if unlocked
			SetMark(attribute_info.m_Mark, true);
		}
		if (!attribute_info.m_bCrystalWeapon)
		{
			if (img_frame != null)
			{
				img_frame.texture = texture_frame_normal;
			}
			if (img_frame_choose != null)
			{
				img_frame_choose.texture = texture_frame_normal_choose;
			}
		}
		else
		{
			if (img_frame != null)
			{
				img_frame.texture = texture_frame_crystal;
			}
			if (img_frame_choose != null)
			{
				img_frame_choose.texture = texture_frame_crystal_choose;
			}
		}
		bool flag = !(attribute_info.m_fDiscount >= 1f);
		bool bActive = attribute_info.m_bActive;
		if (flag || bActive)
		{
			if (img_sale != null)
			{
				img_sale.gameObject.SetActiveRecursive(true);
				if (bActive)
				{
					img_sale.texture = img_sale_sign_texture02;
				}
				else
				{
					img_sale.texture = img_sale_sign_texture01;
				}
			}
			if (label_sale != null)
			{
				label_sale.gameObject.SetActiveRecursive(true);
				if (bActive)
				{
					label_sale.Text = "Free!!!";
				}
			}
		}
		else
		{
			if (img_sale != null)
			{
				img_sale.gameObject.SetActiveRecursive(false);
			}
			if (label_sale != null)
			{
				label_sale.gameObject.SetActiveRecursive(false);
			}
		}
	}

	public void SetCustomizeTexture(TUIMeshSprite m_sprite, string m_path, bool m_use_NGUI = false)
	{
		m_sprite.texture = string.Empty;
		m_sprite.UseCustomize = true;
		m_sprite.CustomizeTexture = Resources.Load(m_path) as Texture;
		if (m_sprite.CustomizeTexture == null)
		{
			Debug.LogWarning("lose texture!" + m_path);
			return;
		}
		if (!m_use_NGUI)
		{
			m_sprite.CustomizeRect = new Rect(0f, 0f, m_sprite.CustomizeTexture.width, m_sprite.CustomizeTexture.height);
			return;
		}
		Rect rect = new Rect(0f, 0f, m_sprite.CustomizeTexture.width, m_sprite.CustomizeTexture.height);
		Common.GetAtlasSpriteSize(NGUI_weapon_altas_path + m_sprite.CustomizeTexture.name, m_sprite.CustomizeTexture.name, ref rect);
		m_sprite.CustomizeRect = rect;
	}

	public TUIMeshSprite GetCustomizeTexture()
	{
		return img_bg;
	}

	public void OnSelect()
	{
		if (!be_choose)
		{
			be_choose = true;
			img_frame.gameObject.SetActiveRecursive(false);
			img_frame_choose.gameObject.SetActiveRecursive(true);
		}
	}

	public void OnUnselect()
	{
		if (be_choose)
		{
			be_choose = false;
			img_frame.gameObject.SetActiveRecursive(true);
			img_frame_choose.gameObject.SetActiveRecursive(false);
		}
	}

	public TUIWeaponAttributeInfo GetWeaponAttributeInfo()
	{
		return attribute_info;
	}

	public void SetMark(NewMarkType mark, bool isUnlocked = true)
	{
		if (img_new == null) return;
		if (!isUnlocked || (attribute_info != null && !attribute_info.m_bUnlock))
		{
			img_new.texture = string.Empty;
			img_new.gameObject.SetActiveRecursive(false);
			m_MarkType = NewMarkType.None;
			return;
		}
		switch (mark)
		{
			case NewMarkType.Mark:
				img_new.texture = texture_mark;
				img_new.gameObject.SetActiveRecursive(true);
				break;
			case NewMarkType.New:
				img_new.texture = texture_new;
				img_new.gameObject.SetActiveRecursive(true);
				break;
			default:
				img_new.texture = string.Empty;
				img_new.gameObject.SetActiveRecursive(false);
				break;
		}
		m_MarkType = mark;
	}

	public void RefreshMark()
	{
		if (attribute_info != null)
		{
			SetMark(attribute_info.m_Mark, attribute_info.m_bUnlock);
		}
	}

	public void ShowSaleSign(bool m_show)
	{
		if (img_sale != null)
		{
			img_sale.gameObject.SetActiveRecursive(m_show);
		}
		if (label_sale != null)
		{
			label_sale.gameObject.SetActiveRecursive(m_show);
		}
	}

	public void PlayPurchaseAnimation()
	{
		if (img_bg != null && img_bg.gameObject != null)
		{
			Animation anim = img_bg.gameObject.GetComponent<Animation>();
			if (anim != null && anim.gameObject.activeInHierarchy)
			{
				anim.Play();
				Debug.Log("Playing purchase animation on Img_bg");
			}
			else
			{
				Debug.LogWarning("No Animation component found on Img_bg or its parent");
			}
		}
		else
		{
			Debug.LogWarning("img_bg is null or inactive");
		}
	}
}