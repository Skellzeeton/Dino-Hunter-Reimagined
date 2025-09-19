/* public class MenuScenesSounds : MonoBehaviour
{
    public TUIFade m_fade;

    private float m_fade_in_time;

    private float m_fade_out_time;

    private bool do_fade_in;

    private bool is_fade_out;

    private bool do_fade_out;

    private string next_scene = "Scene_MainMenu";

    private int next_scene_id;

    private bool is_enter_level_scene;

    private bool sfx_open_now = true;

    private bool music_open_now = true;

    public TUILabel label_text;

    public PopupIAP popup_warning;

    private bool connect_success;

    private ServerConnectFailType server_connect_fail;

    public GameObject prefab_popup_server;

    private PopupServer popup_server;

    public Transform tui_control;

    private bool didTheThing;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        TUIDataServer.Instance().Initialize();
        //global::EventCenter.EventCenter.Instance.Register<TUIEvent.BackEvent_SceneMain>(TUIEvent_SetUIInfo);
        label_text.Text = "Touch to play";
    }

    private void Start()
    {
        global::EventCenter.EventCenter.Instance.Publish(this, new TUIEvent.SendEvent_SceneMain(TUIEvent.SceneMainEventType.TUIEvent_EnterInfo));
        if (music_open_now)
        {
            CUISound.GetInstance().Play("BGM_startscreen");
        }
    }*/