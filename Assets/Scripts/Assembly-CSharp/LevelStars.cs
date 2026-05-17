using UnityEngine;

public class LevelStars : MonoBehaviour
{
    public TUIMeshSprite star01;
    public TUIMeshSprite star02;
    public TUIMeshSprite star03;
    public TUIMeshSprite star04;
    public TUIMeshSprite star05;

    private string texture_empty = "shenjixingxing2";
    private string texture_full = "shenjixingxing1";

    public GameObject prefab_star_blink;

    private void Start()
    {
        if (texture_empty == string.Empty || texture_full == string.Empty)
        {
            Debug.LogWarning("no texture!");
            return;
        }
        star01.texture = texture_empty;
        star02.texture = texture_empty;
        star03.texture = texture_empty;
        star04.texture = texture_empty;
        star05.texture = texture_empty;
    }

    public void SetStars(int count, Vector3 m_position, int blink_index = 0)
    {
        if (texture_empty == string.Empty || texture_full == string.Empty)
        {
            Debug.LogWarning("no texture!");
            return;
        }
        gameObject.SetActive(true);
        gameObject.transform.localPosition = m_position;
        SetStarTextures(count);
        HandleBlink(blink_index);
    }

    public void SetStars(int currentLevel, int maxLevel, Vector3 position, int blinkIndex = 0)
    {
        if (texture_empty == string.Empty || texture_full == string.Empty)
        {
            Debug.LogWarning("no texture!");
            return;
        }
        gameObject.SetActive(true);
        gameObject.transform.localPosition = position;
        star01.gameObject.SetActive(maxLevel >= 1);
        star02.gameObject.SetActive(maxLevel >= 2);
        star03.gameObject.SetActive(maxLevel >= 3);
        star04.gameObject.SetActive(maxLevel >= 4);
        star05.gameObject.SetActive(maxLevel >= 5);
        SetStarTextures(currentLevel);
        HandleBlink(blinkIndex);
    }

    public void SetStars(int count)
    {
        if (texture_empty == string.Empty || texture_full == string.Empty)
        {
            Debug.LogWarning("no texture!");
            return;
        }
        gameObject.SetActive(true);
        SetStarTextures(count);
    }

    public void SetStarsDisable()
    {
        gameObject.SetActive(false);
    }

    private void SetStarTextures(int count)
    {
        switch (count)
        {
            case 0:
                star01.texture = texture_empty;
                star02.texture = texture_empty;
                star03.texture = texture_empty;
                star04.texture = texture_empty;
                star05.texture = texture_empty;
                break;
            case 1:
                star01.texture = texture_full;
                star02.texture = texture_empty;
                star03.texture = texture_empty;
                star04.texture = texture_empty;
                star05.texture = texture_empty;
                break;
            case 2:
                star01.texture = texture_full;
                star02.texture = texture_full;
                star03.texture = texture_empty;
                star04.texture = texture_empty;
                star05.texture = texture_empty;
                break;
            case 3:
                star01.texture = texture_full;
                star02.texture = texture_full;
                star03.texture = texture_full;
                star04.texture = texture_empty;
                star05.texture = texture_empty;
                break;
            case 4:
                star01.texture = texture_full;
                star02.texture = texture_full;
                star03.texture = texture_full;
                star04.texture = texture_full;
                star05.texture = texture_empty;
                break;
            case 5:
                star01.texture = texture_full;
                star02.texture = texture_full;
                star03.texture = texture_full;
                star04.texture = texture_full;
                star05.texture = texture_full;
                break;
        }
    }

    private void HandleBlink(int blinkIndex)
    {
        if (prefab_star_blink == null) return;
        TUIMeshSprite targetStar = null;
        switch (blinkIndex)
        {
            case 1: targetStar = star01; break;
            case 2: targetStar = star02; break;
            case 3: targetStar = star03; break;
            case 4: targetStar = star04; break;
            case 5: targetStar = star05; break;
            default: return;
        }
        if (targetStar == null) return;
        GameObject blinkObj = (GameObject)Object.Instantiate(prefab_star_blink);
        blinkObj.transform.parent = targetStar.transform;
        blinkObj.transform.localPosition = new Vector3(0f, 0f, -1f);
        blinkObj.GetComponent<StarBlink>().ShowBlink();
    }
}