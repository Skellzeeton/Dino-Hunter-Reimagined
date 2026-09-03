using UnityEngine;

public class BtnSelect_Item : MonoBehaviour
{
	public TUIMeshSprite img_item;

	public TUIMeshSprite img_new;

	private int index;

	private TUIPopupInfo popup_info;

	private string m_sPathRootCustomTex = "Artist/Textrues";

	private string m_sPathRootCustomAtlas = "Artist/Atlas";

	private string texture_mark = "new";

	private string texture_new = "new2";

	private string skill_path = "TUI/Skill/";

	private void SetCustomizeTexture(TUIMeshSprite m_sprite, string m_path, bool m_use_NGUI = false, string pathforatlas = "")
	{
		m_sprite.texture = string.Empty;
		m_sprite.UseCustomize = true;
		m_sprite.CustomizeTexture = Resources.Load(m_path) as Texture;
		if (m_sprite.CustomizeTexture == null)
		{
			Debug.LogWarning("lose texture!");
			return;
		}
		if (!m_use_NGUI)
		{
			m_sprite.CustomizeRect = new Rect(0f, 0f, m_sprite.CustomizeTexture.width, m_sprite.CustomizeTexture.height);
			return;
		}
		Rect rect = new Rect(0f, 0f, m_sprite.CustomizeTexture.width, m_sprite.CustomizeTexture.height);
		Common.GetAtlasSpriteSize(pathforatlas, m_sprite.CustomizeTexture.name, ref rect);
		m_sprite.CustomizeRect = rect;
	}

	private void LoadSkillTexture(TUIMeshSprite sprite, int skillId)
	{
		if (sprite == null)
			return;
		string textureName = TUIMappingInfo.Instance().GetSkillTexture(skillId);
		Texture tex = Resources.Load(skill_path + textureName) as Texture;

		if (tex != null)
		{
			sprite.UseCustomize = true;
			sprite.CustomizeTexture = tex;
			sprite.CustomizeRect = new Rect(0f, 0f, tex.width, tex.height);
			sprite.texture = string.Empty;
		}
		else
		{
			sprite.texture = textureName;
			Debug.LogWarning("Failed to load skill texture from Resources: " + skill_path + textureName);
		}
	}

	public void SetClipRect(TUIRect rect)
	{
		base.transform.GetComponent<TUIClipBinder>().SetClipRect(rect);
	}

	public void DoCreate(TUIPopupInfo info, TUIRect rect, int m_index)
	{
		index = m_index;
		popup_info = info;
		SetClipRect(rect);
		SetMark(info.m_MarkType);
		if (info == null)
		{
			return;
		}
		if (info.IsWeapon() || info.IsArmor() || info.IsAccessory())
		{
			string text = info.m_sIcon;
			if (text.Length < 1)
			{
				text = TUIMappingInfo.Instance().GetWeaponTexture(info.texture_id);
			}
			string path = string.Empty;
			bool use_NGUI = false;
			string pathforatlas = string.Empty;
			if (info.IsWeapon())
			{
				path = m_sPathRootCustomTex + "/Weapon/" + text;
				use_NGUI = true;
				pathforatlas = m_sPathRootCustomAtlas + "/Weapon/" + text;
			}
			else if (info.IsArmor())
			{
				path = m_sPathRootCustomTex + "/Armor/" + text;
			}
			else if (info.IsAccessory())
			{
				path = m_sPathRootCustomTex + "/Accessory/" + text;
			}
			SetCustomizeTexture(img_item, path, use_NGUI, pathforatlas);
			SetCustomizeTexture(img_item, path, use_NGUI, pathforatlas);
		}
		else if (info.IsProps())
		{
			string propTexture = TUIMappingInfo.Instance().GetPropTexture(popup_info.texture_id);
			img_item.texture = propTexture;
		}
		else if (info.IsSkill())
		{
			LoadSkillTexture(img_item, popup_info.texture_id);
		}
		else if (info.IsRole())
		{
			string roleTexture = TUIMappingInfo.Instance().GetRoleTexture(popup_info.texture_id);
			img_item.texture = roleTexture;
		}
	}

	public int GetIndex()
	{
		return index;
	}

	public string GetIntroduce()
	{
		return popup_info.introduce;
	}

	public TUIPopupInfo GetInfo()
	{
		return popup_info;
	}

	public void SetMark(NewMarkType mark)
	{
		if (popup_info != null && !(img_new == null))
		{
			popup_info.m_MarkType = mark;
			switch (mark)
			{
				case NewMarkType.New:
					img_new.texture = texture_new;
					break;
				case NewMarkType.Mark:
					img_new.texture = texture_mark;
					break;
				default:
					img_new.texture = string.Empty;
					break;
			}
		}
	}

	public NewMarkType GetMark()
	{
		if (popup_info == null)
		{
			return NewMarkType.None;
		}
		return popup_info.m_MarkType;
	}
}