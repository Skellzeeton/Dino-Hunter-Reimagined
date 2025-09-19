using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Atlas")]
public class UIAtlas : MonoBehaviour
{
    [Serializable]
    public class Sprite
    {
        public string name = "Unity Bug";
        public Rect outer = new Rect(0f, 0f, 1f, 1f);
        public Rect inner = new Rect(0f, 0f, 1f, 1f);
        public float paddingLeft;
        public float paddingRight;
        public float paddingTop;
        public float paddingBottom;

        public bool hasPadding
        {
            get
            {
                return paddingLeft != 0f || paddingRight != 0f || paddingTop != 0f || paddingBottom != 0f;
            }
        }
    }

    public enum Coordinates
    {
        Pixels,
        TexCoords
    }

    [SerializeField]
    private Material material;

    [SerializeField]
    private List<Sprite> sprites = new List<Sprite>();

    [SerializeField]
    private Coordinates mCoordinates;
    
    [SerializeField]
    private float mPixelSize = 1f;

    [SerializeField]
    private UIAtlas mReplacement;

    public Material spriteMaterial
    {
        get
        {
            return (mReplacement != null) ? mReplacement.spriteMaterial : material;
        }
        set
        {
            if (mReplacement != null)
            {
                mReplacement.spriteMaterial = value;
                return;
            }
            if (material != value)
            {
                MarkAsDirty();
                material = value;
                MarkAsDirty();
            }
        }
    }

    public List<Sprite> spriteList
    {
        get { return (mReplacement != null) ? mReplacement.spriteList : sprites; }
        set
        {
            if (mReplacement != null)
                mReplacement.spriteList = value;
            else
                sprites = value;
        }
    }

    public Texture texture
    {
        get
        {
            return (mReplacement != null) ? mReplacement.texture :
                   (material != null ? material.mainTexture : null);
        }
    }

    public Coordinates coordinates
    {
        get { return (mReplacement != null) ? mReplacement.coordinates : mCoordinates; }
        set
        {
            if (mReplacement != null)
            {
                mReplacement.coordinates = value;
            }
            else if (mCoordinates != value)
            {
                if (material == null || material.mainTexture == null)
                {
                    Debug.LogError("Can't switch coordinates until the atlas material has a valid texture");
                    return;
                }

                mCoordinates = value;
                Texture tex = material.mainTexture;
                foreach (var sprite in sprites)
                {
                    if (mCoordinates == Coordinates.TexCoords)
                    {
                        sprite.outer = NGUIMath.ConvertToTexCoords(sprite.outer, tex.width, tex.height);
                        sprite.inner = NGUIMath.ConvertToTexCoords(sprite.inner, tex.width, tex.height);
                    }
                    else
                    {
                        sprite.outer = NGUIMath.ConvertToPixels(sprite.outer, tex.width, tex.height, true);
                        sprite.inner = NGUIMath.ConvertToPixels(sprite.inner, tex.width, tex.height, true);
                    }
                }
            }
        }
    }

    public float pixelSize
    {
        get { return (mReplacement != null) ? mReplacement.pixelSize : mPixelSize; }
        set
        {
            if (mReplacement != null)
                mReplacement.pixelSize = value;
            else
            {
                float val = Mathf.Clamp(value, 0.25f, 4f);
                if (mPixelSize != val)
                {
                    mPixelSize = val;
                    MarkAsDirty();
                }
            }
        }
    }

    public UIAtlas replacement
    {
        get { return mReplacement; }
        set
        {
            UIAtlas val = (value == this) ? null : value;
            if (mReplacement != val)
            {
                if (val != null && val.replacement == this)
                    val.replacement = null;

                if (mReplacement != null)
                    MarkAsDirty();

                mReplacement = val;
                MarkAsDirty();
            }
        }
    }

    public Sprite GetSprite(string name)
    {
        if (mReplacement != null)
            return mReplacement.GetSprite(name);

        if (!string.IsNullOrEmpty(name))
        {
            foreach (var sprite in sprites)
            {
                if (!string.IsNullOrEmpty(sprite.name) && sprite.name == name)
                    return sprite;
            }
        }
        else
        {
            Debug.LogWarning("Expected a valid name, found nothing");
        }
        return null;
    }

    public BetterList<string> GetListOfSprites()
    {
        if (mReplacement != null)
            return mReplacement.GetListOfSprites();

        BetterList<string> list = new BetterList<string>();
        foreach (var sprite in sprites)
        {
            if (!string.IsNullOrEmpty(sprite.name))
                list.Add(sprite.name);
        }
        return list;
    }

    public BetterList<string> GetListOfSprites(string match)
    {
        if (mReplacement != null)
            return mReplacement.GetListOfSprites(match);

        if (string.IsNullOrEmpty(match))
            return GetListOfSprites();

        BetterList<string> result = new BetterList<string>();

        foreach (var sprite in sprites)
        {
            if (!string.IsNullOrEmpty(sprite.name) &&
                string.Equals(match, sprite.name, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(sprite.name);
                return result;
            }
        }

        string[] keywords = match.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var sprite in sprites)
        {
            string lowerName = sprite.name.ToLower();
            int matches = 0;
            foreach (var word in keywords)
                if (lowerName.Contains(word)) matches++;
            if (matches == keywords.Length)
                result.Add(sprite.name);
        }

        return result;
    }

    private bool References(UIAtlas atlas)
    {
        return atlas != null && (atlas == this || (mReplacement != null && mReplacement.References(atlas)));
    }

    public static bool CheckIfRelated(UIAtlas a, UIAtlas b)
    {
        if (a == null || b == null)
            return false;
        return a == b || a.References(b) || b.References(a);
    }

    public void MarkAsDirty()
    {
        if (mReplacement != null)
            mReplacement.MarkAsDirty();

        UISprite[] sprites = NGUITools.FindActive<UISprite>();
        foreach (var sprite in sprites)
        {
            if (CheckIfRelated(this, sprite.atlas))
            {
                UIAtlas temp = sprite.atlas;
                sprite.atlas = null;
                sprite.atlas = temp;
            }
        }

        UIFont[] fonts = Resources.FindObjectsOfTypeAll(typeof(UIFont)) as UIFont[];
        foreach (var font in fonts)
        {
            if (CheckIfRelated(this, font.atlas))
            {
                UIAtlas temp = font.atlas;
                font.atlas = null;
                font.atlas = temp;
            }
        }

        UILabel[] labels = NGUITools.FindActive<UILabel>();
        foreach (var label in labels)
        {
            if (label.font != null && CheckIfRelated(this, label.font.atlas))
            {
                UIFont temp = label.font;
                label.font = null;
                label.font = temp;
            }
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Make sure Inspector updates trigger MarkAsDirty()
        spriteMaterial = material;
    }
#endif
}
