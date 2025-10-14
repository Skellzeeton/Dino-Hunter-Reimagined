using UnityEngine;

public static class iGlobalResume
{
    public static void Force()
    {
        Time.timeScale = 1f;
        GamePause.IsPaused = false;
        ParticleSystem[] allParticles = UnityEngine.Object.FindObjectsOfType<ParticleSystem>();
        foreach (ParticleSystem ps in allParticles)
        {
            if (ps != null) ps.Play();
        }
        iGameState gameState = iGameApp.GetInstance().m_GameState;
        if (gameState != null)
        {
            for (int i = 0; i < 3; i++)
            {
                CWeaponBase weapon = gameState.GetWeapon(i);
                if (weapon != null)
                    weapon.PauseFire(false);
            }
        }
    }
}