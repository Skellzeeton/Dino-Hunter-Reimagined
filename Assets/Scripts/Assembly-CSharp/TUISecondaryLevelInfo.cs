using System.Collections.Generic;

public class TUISecondaryLevelInfo
{
    public int id;

    public string title;

    public string introduce01;

    public string introduce02;

    public List<TUIGoodsInfo> goods_drop_list;

    public Dictionary<int, GoodsQualityType> goods_id_list;

    public TUIRecommendRoleInfo recommend_role_info;

    public TUIRecommendWeaponInfo recommend_weapon_info;

    public SecondaryLevelType level_type;

    public LevelPassState pass_state;

    // Full constructor
    public TUISecondaryLevelInfo(
        int m_id,
        string m_introduce01,
        List<TUIGoodsInfo> m_goods_drop_list,
        SecondaryLevelType m_level_type,
        LevelPassState m_pass_state)
    {
        id = m_id;
        title = string.Empty;
        introduce01 = m_introduce01 != null ? m_introduce01 : string.Empty;
        introduce02 = string.Empty;
        goods_drop_list = m_goods_drop_list != null ? m_goods_drop_list : new List<TUIGoodsInfo>();
        level_type = m_level_type;
        pass_state = m_pass_state;
        recommend_role_info = null;
        recommend_weapon_info = null;
        goods_id_list = new Dictionary<int, GoodsQualityType>();
    }


    // Simplified constructor without recommendations or goods_id_list
    public TUISecondaryLevelInfo(
        int m_id,
        string m_title,
        string m_introduce01,
        string m_introduce02,
        List<TUIGoodsInfo> m_goods_drop_list,
        SecondaryLevelType m_level_type,
        LevelPassState m_pass_state)
    {
        id = m_id;
        title = m_title != null ? m_title : string.Empty;
        introduce01 = m_introduce01 != null ? m_introduce01 : string.Empty;
        introduce02 = m_introduce02 != null ? m_introduce02 : string.Empty;
        goods_drop_list = m_goods_drop_list != null ? m_goods_drop_list : new List<TUIGoodsInfo>();
        level_type = m_level_type;
        pass_state = m_pass_state;
        recommend_role_info = null;
        recommend_weapon_info = null;
        goods_id_list = new Dictionary<int, GoodsQualityType>();
    }
}
