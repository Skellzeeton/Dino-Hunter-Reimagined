using UnityEngine;

public class LabelInfo_Weapon : MonoBehaviour
{
	public TUILabel label_damage;
	public TUILabel label_damage_value;
	public TUILabel label_fire_rate;
	public TUILabel label_fire_rate_value;
	public TUILabel label_blast_radius;
	public TUILabel label_blast_radius_value;
	public TUILabel label_knockback;
	public TUILabel label_knockback_value;
	public TUILabel label_ammo;
	public TUILabel label_ammo_value;
	public TUILabel label_introduce;
	public TUILabel label_introduce_unlock;
	public TUILabel label_max_value;
	public TUILabel label_unlock_text;
	public TUILabel label_def;
	public TUILabel label_def_value;

	public TUILabel label_crit_chance;
	public TUILabel label_crit_chance_value;
	public TUILabel label_crit_damage;
	public TUILabel label_crit_damage_value;

	public void SetWeaponInfo(float m_damage, float m_fire_rate, int m_blast_radius, int m_knockback, int m_ammo,
			float m_damage_max, float m_crit_chance, float m_crit_damage,
			bool m_unlock, string m_unlock_text)
	{
		SetNull();
		if (!m_unlock)
		{
			label_unlock_text.gameObject.SetActiveRecursive(true);
			label_unlock_text.Text = m_unlock_text;
		}
		else
		{
			label_damage.gameObject.SetActiveRecursive(true);
			label_damage_value.gameObject.SetActiveRecursive(true);
			label_fire_rate.gameObject.SetActiveRecursive(true);
			label_fire_rate_value.gameObject.SetActiveRecursive(true);
			label_blast_radius.gameObject.SetActiveRecursive(true);
			label_blast_radius_value.gameObject.SetActiveRecursive(true);
			label_knockback.gameObject.SetActiveRecursive(true);
			label_knockback_value.gameObject.SetActiveRecursive(true);
			label_ammo.gameObject.SetActiveRecursive(true);
			label_ammo_value.gameObject.SetActiveRecursive(true);
			label_max_value.gameObject.SetActiveRecursive(true);
			label_damage_value.Text = (m_damage == 0f) ? "--" : m_damage.ToString("0.##");
			label_max_value.Text = (m_damage_max == 0f) ? "--" : "(Max " + m_damage_max.ToString("0.##") + ")";
			label_fire_rate_value.Text = (m_fire_rate == 0f) ? "--" : m_fire_rate.ToString();
			label_blast_radius_value.Text = (m_blast_radius == 0) ? "--" : m_blast_radius.ToString();
			label_knockback_value.Text = (m_knockback == 0) ? "--" : m_knockback.ToString();
			label_ammo_value.Text = (m_ammo == 0) ? "--" : m_ammo.ToString();
			label_crit_chance.gameObject.SetActiveRecursive(true);
			label_crit_chance_value.gameObject.SetActiveRecursive(true);
			label_crit_chance_value.Text = (m_crit_chance > 0f) ? m_crit_chance.ToString("0.#") + "%" : "--";
			label_crit_damage.gameObject.SetActiveRecursive(true);
			label_crit_damage_value.gameObject.SetActiveRecursive(true);
			if (m_crit_damage > 0f)
				label_crit_damage_value.Text = "+" + m_crit_damage.ToString("0.#") + "%";
			else
				label_crit_damage_value.Text = "--";
		}
	}

	public void SetWeaponInfo(float m_fire_rate, int m_blast_radius, int m_knockback, int m_ammo,
			float m_damage_max, float m_crit_chance, float m_crit_damage,
			bool m_unlock, string m_unlock_text)
	{
		SetNull();
		if (!m_unlock)
		{
			label_introduce_unlock.gameObject.SetActiveRecursive(true);
			label_introduce_unlock.Text = m_unlock_text;
		}
		else
		{
			label_fire_rate.gameObject.SetActiveRecursive(true);
			label_fire_rate_value.gameObject.SetActiveRecursive(true);
			label_blast_radius.gameObject.SetActiveRecursive(true);
			label_blast_radius_value.gameObject.SetActiveRecursive(true);
			label_knockback.gameObject.SetActiveRecursive(true);
			label_knockback_value.gameObject.SetActiveRecursive(true);
			label_ammo.gameObject.SetActiveRecursive(true);
			label_ammo_value.gameObject.SetActiveRecursive(true);
			label_max_value.gameObject.SetActiveRecursive(true);
			label_max_value.Text = (m_damage_max == 0f) ? "--" : "(Max " + m_damage_max.ToString("0.##") + ")";
			label_fire_rate_value.Text = (m_fire_rate == 0f) ? "--" : m_fire_rate.ToString();
			label_blast_radius_value.Text = (m_blast_radius == 0) ? "--" : m_blast_radius.ToString();
			label_knockback_value.Text = (m_knockback == 0) ? "--" : m_knockback.ToString();
			label_ammo_value.Text = (m_ammo == 0) ? "--" : m_ammo.ToString();
			label_crit_chance.gameObject.SetActiveRecursive(true);
			label_crit_chance_value.gameObject.SetActiveRecursive(true);
			label_crit_chance_value.Text = (m_crit_chance > 0f) ? m_crit_chance.ToString("0.#") + "%" : "--";
			label_crit_damage.gameObject.SetActiveRecursive(true);
			label_crit_damage_value.gameObject.SetActiveRecursive(true);
			if (m_crit_damage > 0f)
				label_crit_damage_value.Text = "+" + m_crit_damage.ToString("0.#") + "%";
			else
				label_crit_damage_value.Text = "--";
		}
	}

	public void SetArmorAccessoryInfo(string m_introduce, int m_def, int m_def_max, bool m_unlock, string m_unlock_text = "")
	{
		SetNull();
		if (!m_unlock)
		{
			label_unlock_text.gameObject.SetActiveRecursive(true);
			label_unlock_text.Text = m_unlock_text;
		}
		else
		{
			label_def.gameObject.SetActiveRecursive(true);
			label_def_value.gameObject.SetActiveRecursive(true);
			label_introduce.gameObject.SetActiveRecursive(true);
			label_max_value.gameObject.SetActiveRecursive(true);
		}
		if (m_def_max == 0)
		{
			label_max_value.Text = "--";
		}
		else
		{
			label_max_value.Text = "(Max " + m_def_max + ")";
		}
		if (m_def == 0)
		{
			label_def_value.Text = "--";
		}
		else
		{
			label_def_value.Text = m_def.ToString();
		}
		label_introduce.Text = m_introduce;
	}

	public void SetDamage(float m_damage)
	{
		if (label_damage != null)
		{
			label_damage.gameObject.SetActiveRecursive(true);
		}
		if (label_damage_value != null)
		{
			label_damage_value.gameObject.SetActiveRecursive(true);
			label_damage_value.Text = m_damage.ToString("0.##");
		}
	}

	public void OpenDamageAnimation()
	{
		if (label_damage_value != null && label_damage_value.GetComponent<Animation>() != null && label_damage_value.gameObject.activeInHierarchy)
		{
			label_damage_value.GetComponent<Animation>().Play();
		}
	}

	public void SetDef(int m_def)
	{
		if (label_def != null)
		{
			label_def.gameObject.SetActiveRecursive(true);
		}
		if (label_def_value != null)
		{
			label_def_value.gameObject.SetActiveRecursive(true);
			if (m_def == 0)
			{
				label_def_value.Text = "--";
			}
			else
			{
				label_def_value.Text = m_def.ToString();
			}
		}
	}

	public void OpenDefAnimation()
	{
		if (label_def_value != null && label_damage_value.gameObject.active && label_def_value.GetComponent<Animation>() != null)
		{
			label_def_value.GetComponent<Animation>().Play();
		}
	}

	public void SetNull()
	{
		TUILabel[] array = new TUILabel[]
		{
			label_damage, label_damage_value,
			label_fire_rate, label_fire_rate_value,
			label_blast_radius, label_blast_radius_value,
			label_knockback, label_knockback_value,
			label_ammo, label_ammo_value,
			label_introduce, label_max_value,
			label_unlock_text, label_def, label_def_value,
			// NEW
			label_crit_chance, label_crit_chance_value,
			label_crit_damage, label_crit_damage_value
		};
		foreach (TUILabel label in array)
		{
			if (label != null)
				label.gameObject.SetActiveRecursive(false);
		}
	}
}