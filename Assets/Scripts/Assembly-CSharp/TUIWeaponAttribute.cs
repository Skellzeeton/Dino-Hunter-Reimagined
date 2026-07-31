public class TUIWeaponAttribute
{
	public float damage;

	public float fire_rate;

	public float blast_radius;

	public float knockback;

	public float ammo;

	public float crit_chance;

	public float crit_damage;

	public TUIWeaponAttribute()
	{
	}

	public TUIWeaponAttribute(float m_damage, float m_fire_rate, float m_blast_radius, float m_knockback, float m_ammo, float m_crit_chance, float m_crit_damage)
	{
		damage = m_damage;
		fire_rate = m_fire_rate;
		blast_radius = m_blast_radius;
		knockback = m_knockback;
		ammo = m_ammo;
		crit_chance = m_crit_chance;
		crit_damage = m_crit_damage;
	}
}
