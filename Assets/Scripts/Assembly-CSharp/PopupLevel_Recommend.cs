using UnityEngine;

public class PopupLevel_Recommend : MonoBehaviour
{
	public enum RecommendType    { None = 0, Role = 1, Weapon = 2 }
	public enum RequiredType     { None = 0, Role = 1, Weapon = 2 }
	public enum RecommendBtnState { Disable = 0, RoleBuy = 1, RoleEquip = 2, WeaponBuy = 3, WeaponEquip = 4 }

	public TUIMeshSprite   img_role;
	public TUIMeshSprite   img_weapon;
	public LevelStars      level_stars;
	public TUILabel        label_recommend_title;
	public TUIButtonClick  btn_buy;
	public TUILabel        label_btn_buy_normal;
	public TUILabel        label_btn_buy_press;
	private string NGUI_weapon_atlas_path = "Artist/Atlas/Weapon/";
	private Vector3 role_normal_pos   = Vector3.zero;
	private Vector3 weapon_normal_pos = Vector3.zero;
	private Vector3 stars_normal_pos  = Vector3.zero;
	private Vector3 delta_pos         = Vector3.zero;
	private const float TITLE_X_RECOMMENDED = -17.5f;
	private const float TITLE_X_REQUIRED    = -5f;
	private RecommendType     recommend_type;
	private RequiredType      required_type;
	private RecommendBtnState recommend_btn_state;
	private TUIRecommendRoleInfo   recommend_role;
	private TUIRecommendWeaponInfo recommend_weapon;
	private bool open_start = true;

	private void Awake()
	{
		delta_pos = new Vector3(27f, 0f, 0f);
		if (img_role    != null) role_normal_pos   = img_role.transform.localPosition;
		if (img_weapon  != null) weapon_normal_pos  = img_weapon.transform.localPosition;
		if (level_stars != null) stars_normal_pos   = level_stars.transform.localPosition;
	}

	public RecommendType      GetRecommendType()     { return recommend_type; }
	public RequiredType       GetRequiredType()      { return required_type; }
	public bool               GetOpenStart()         { return open_start; }
	public RecommendBtnState  GetRecommendBtnState() { return recommend_btn_state; }
	public TUIRecommendWeaponInfo GetRecommendWeaponInfo() { return recommend_weapon; }
	public TUIRecommendRoleInfo   GetRecommendRoleInfo()   { return recommend_role; }

	public void SetRecommendNone()
	{
		open_start     = true;
		recommend_type = RecommendType.None;
		required_type  = RequiredType.None;

		label_recommend_title.Text  = string.Empty;
		img_role.texture            = string.Empty;
		img_weapon.UseCustomize     = false;
		img_weapon.CustomizeTexture = null;

		img_role.gameObject.SetActiveRecursive(false);
		img_role.transform.localPosition   = role_normal_pos;
		img_weapon.gameObject.SetActiveRecursive(false);
		img_weapon.transform.localPosition  = weapon_normal_pos;
		level_stars.gameObject.SetActiveRecursive(false);
		level_stars.transform.localPosition = stars_normal_pos;
		btn_buy.gameObject.SetActiveRecursive(false);
	}

	public void SetRecommendWeapon(TUIRecommendWeaponInfo m_recommend_weapon)
	{
		open_start = true;
		if (m_recommend_weapon == null) return;
		recommend_weapon = m_recommend_weapon;
		recommend_type   = RecommendType.Weapon;
		bool required   = m_recommend_weapon.required;
		int  level_need = m_recommend_weapon.level_need;
		int  level      = m_recommend_weapon.level;
		bool have_equip = m_recommend_weapon.have_equip;
		int  id         = m_recommend_weapon.id;
		SetTitleText(required);
		required_type = required ? RequiredType.Weapon : RequiredType.None;
		img_role.texture = string.Empty;
		img_role.gameObject.SetActiveRecursive(false);
		string texName = TUIMappingInfo.Instance().GetWeaponTexture(id);
		string texPath = TUIMappingInfo.Instance().m_sPathRootCustomTex + "/Weapon/" + texName;
		SetAtlasTexture(img_weapon, texPath, NGUI_weapon_atlas_path);
		img_weapon.gameObject.SetActiveRecursive(true);
		ApplyStarsAndButtons(level, level_need, have_equip, required);
		UpdateRequiredAni();
	}

	public void SetRecommendRole(TUIRecommendRoleInfo m_recommend_role)
	{
		open_start     = true;
		recommend_role = m_recommend_role;
		recommend_type = RecommendType.Role;
		bool required   = m_recommend_role.required;
		bool have_buy   = m_recommend_role.have_buy;
		bool have_equip = m_recommend_role.have_equip;
		int  id         = m_recommend_role.id;
		SetTitleText(required);
		required_type = required ? RequiredType.Role : RequiredType.None;
		img_weapon.UseCustomize     = false;
		img_weapon.CustomizeTexture = null;
		img_weapon.gameObject.SetActiveRecursive(false);
		level_stars.gameObject.SetActiveRecursive(false);
		img_role.texture = TUIMappingInfo.Instance().GetRoleTexture(id);
		img_role.gameObject.SetActiveRecursive(true);
		if (!have_buy)
		{
			label_btn_buy_normal.Text = "Buy";
			label_btn_buy_press.Text  = "Buy";
			btn_buy.gameObject.SetActiveRecursive(true);
			btn_buy.Show();
			recommend_btn_state = RecommendBtnState.RoleBuy;
			img_role.transform.localPosition = role_normal_pos;
			if (required) open_start = false;
		}
		else if (!have_equip)
		{
			label_btn_buy_normal.Text = "Equip";
			label_btn_buy_press.Text  = "Equip";
			btn_buy.gameObject.SetActiveRecursive(true);
			btn_buy.Show();
			recommend_btn_state = RecommendBtnState.RoleEquip;
			img_role.transform.localPosition = role_normal_pos;
			if (required) open_start = false;
		}
		else
		{
			btn_buy.gameObject.SetActiveRecursive(false);
			recommend_btn_state = RecommendBtnState.Disable;
			img_role.transform.localPosition = role_normal_pos + delta_pos;
		}
		UpdateRequiredAni();
	}

	public void SetRecommendAvatarItem(TUIRecommendWeaponInfo m_recommend_weapon)
	{
		open_start = true;
		if (m_recommend_weapon == null) return;
		recommend_weapon = m_recommend_weapon;
		recommend_type   = RecommendType.Weapon;
		bool required   = m_recommend_weapon.required;
		int  level_need = m_recommend_weapon.level_need;
		int  level      = m_recommend_weapon.level;
		bool have_equip = m_recommend_weapon.have_equip;
		int  id         = m_recommend_weapon.id;
		SetTitleText(required);
		required_type = required ? RequiredType.Weapon : RequiredType.None;
		img_role.texture = string.Empty;
		img_role.gameObject.SetActiveRecursive(false);
		string texPath = BuildAvatarTexPath(id);
		SetDirectTexture(img_weapon, texPath);
		img_weapon.gameObject.SetActiveRecursive(true);
		ApplyStarsAndButtons(level, level_need, have_equip, required);
		UpdateRequiredAni();
	}

	private void SetTitleText(bool required)
	{
		if (label_recommend_title == null) return;
		label_recommend_title.Text = required ? "Required" : "Recommended";
		Vector3 pos = label_recommend_title.transform.localPosition;
		pos.x = required ? TITLE_X_REQUIRED : TITLE_X_RECOMMENDED;
		label_recommend_title.transform.localPosition = pos;
	}

	private void ApplyStarsAndButtons(int level, int level_need, bool have_equip, bool required)
	{
		level_stars.SetStars(level_need);
		level_stars.gameObject.SetActiveRecursive(true);
		level_stars.transform.localPosition = stars_normal_pos;

		if (level < level_need)
		{
			label_btn_buy_normal.Text = "Buy";
			label_btn_buy_press.Text  = "Buy";
			btn_buy.gameObject.SetActiveRecursive(true);
			btn_buy.Show();
			recommend_btn_state = RecommendBtnState.WeaponBuy;
			img_weapon.transform.localPosition = weapon_normal_pos;
			if (required) open_start = false;
		}
		else
		{
			if (level_need == 0 && required)
				level_stars.gameObject.SetActiveRecursive(false);

			if (!have_equip)
			{
				label_btn_buy_normal.Text = "Equip";
				label_btn_buy_press.Text  = "Equip";
				btn_buy.gameObject.SetActiveRecursive(true);
				btn_buy.Show();
				recommend_btn_state = RecommendBtnState.WeaponEquip;
				img_weapon.transform.localPosition = weapon_normal_pos;
				if (required) open_start = false;
			}
			else
			{
				btn_buy.gameObject.SetActiveRecursive(false);
				recommend_btn_state = RecommendBtnState.Disable;
				img_weapon.transform.localPosition  = weapon_normal_pos + delta_pos;
				level_stars.transform.localPosition = stars_normal_pos  + delta_pos;
			}
		}
	}

	private string BuildAvatarTexPath(int id)
	{
		iGameData gameData = iGameApp.GetInstance().m_GameData;
		if (gameData != null && gameData.m_AvatarCenter != null)
		{
			CAvatarInfo info = gameData.m_AvatarCenter.Get(id);
			if (info != null && info.m_sIcon.Length > 0)
				return TUIMappingInfo.Instance().m_sPathRootCustomTex + "/Accessory/" + info.m_sIcon;
		}
		string texName = TUIMappingInfo.Instance().GetWeaponTexture(id);
		return TUIMappingInfo.Instance().m_sPathRootCustomTex + "/Weapon/" + texName;
	}

	private void SetDirectTexture(TUIMeshSprite sprite, string texPath)
	{
		sprite.texture          = string.Empty;
		sprite.UseCustomize     = true;
		sprite.CustomizeTexture = Resources.Load(texPath) as Texture;
		if (sprite.CustomizeTexture == null && texPath.Contains("/Accessory/"))
		{
			string armorPath = texPath.Replace("/Accessory/", "/Armor/");
			sprite.CustomizeTexture = Resources.Load(armorPath) as Texture;
			if (sprite.CustomizeTexture != null)
				texPath = armorPath;
		}
		if (sprite.CustomizeTexture == null)
		{
			Debug.LogWarning("[Recommend] lose texture! path=" + texPath);
			return;
		}
		sprite.CustomizeRect = new Rect(0f, 0f,
			sprite.CustomizeTexture.width,
			sprite.CustomizeTexture.height);
	}

	private void SetAtlasTexture(TUIMeshSprite sprite, string texPath, string atlasPath)
	{
		sprite.texture          = string.Empty;
		sprite.UseCustomize     = true;
		sprite.CustomizeTexture = Resources.Load(texPath) as Texture;
		if (sprite.CustomizeTexture == null)
		{
			Debug.LogWarning("[Recommend] lose texture! path=" + texPath);
			return;
		}
		Rect rect = new Rect(0f, 0f, sprite.CustomizeTexture.width, sprite.CustomizeTexture.height);
		Common.GetAtlasSpriteSize(atlasPath + sprite.CustomizeTexture.name,
		                          sprite.CustomizeTexture.name, ref rect);
		sprite.CustomizeRect = rect;
	}

	public void UpdateRequiredAni()
	{
		if (img_role   != null && img_role.GetComponent<Animation>()   != null)
			img_role.GetComponent<Animation>().Stop();
		if (img_weapon != null && img_weapon.GetComponent<Animation>() != null)
			img_weapon.GetComponent<Animation>().Stop();

		if (open_start) return;

		if (required_type == RequiredType.Role)
		{
			if (img_role != null && img_role.GetComponent<Animation>() != null)
			{
				img_role.GetComponent<Animation>().wrapMode = WrapMode.Loop;
				img_role.GetComponent<Animation>().Play();
			}
		}
		else if (required_type == RequiredType.Weapon)
		{
			if (img_weapon != null && img_weapon.GetComponent<Animation>() != null)
			{
				img_weapon.GetComponent<Animation>().wrapMode = WrapMode.Loop;
				img_weapon.GetComponent<Animation>().Play();
			}
		}
	}
}