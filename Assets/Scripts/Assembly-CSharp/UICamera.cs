using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("NGUI/UI/Camera")]
[ExecuteInEditMode]
public class UICamera : MonoBehaviour
{
    public enum ClickNotification
    {
        None,
        Always,
        BasedOnDelta
    }

    public class MouseOrTouch
    {
        public Vector2 pos;
        public Vector2 delta;
        public Vector2 totalDelta;
        public Camera pressedCam;
        public GameObject current;
        public GameObject pressed;
        public float clickTime;
        public ClickNotification clickNotification = ClickNotification.Always;
        public bool touchBegan = true;
        public bool dragStarted;
    }

    private class Highlighted
    {
        public GameObject go;
        public int counter;
    }

    public bool useMouse = true;
    public bool useTouch = true;
    public bool allowMultiTouch = true;
    public bool useKeyboard = true;
    public bool useController = true;
    public LayerMask eventReceiverMask = -1;
    public bool clipRaycasts = true;
    public float tooltipDelay = 1f;
    public bool stickyTooltip = true;
    public float mouseDragThreshold = 4f;
    public float mouseClickThreshold = 10f;
    public float touchDragThreshold = 40f;
    public float touchClickThreshold = 40f;
    public float rangeDistance = -1f;
    public string scrollAxisName = "Mouse ScrollWheel";
    public string verticalAxisName = "Vertical";
    public string horizontalAxisName = "Horizontal";
    public KeyCode submitKey0 = KeyCode.Return;
    public KeyCode submitKey1 = KeyCode.JoystickButton0;
    public KeyCode cancelKey0 = KeyCode.Escape;
    public KeyCode cancelKey1 = KeyCode.JoystickButton1;

    public static bool showTooltips = true;
    public static Vector2 lastTouchPosition = Vector2.zero;
    public static RaycastHit lastHit;
    public static UICamera current = null;
    public static Camera currentCamera = null;
    public static int currentTouchID = -1;
    public static MouseOrTouch currentTouch = null;
    public static bool inputHasFocus = false;
    public static GameObject genericEventHandler;
    public static GameObject fallThrough;

    private static List<UICamera> mList = new List<UICamera>();
    private static List<Highlighted> mHighlighted = new List<Highlighted>();
    private static GameObject mSel = null;
    private static MouseOrTouch[] mMouse = new MouseOrTouch[3]
    {
        new MouseOrTouch(),
        new MouseOrTouch(),
        new MouseOrTouch()
    };
    private static GameObject mHover;
    private static MouseOrTouch mController = new MouseOrTouch();
    private static float mNextEvent = 0f;
    private Dictionary<int, MouseOrTouch> mTouches = new Dictionary<int, MouseOrTouch>();
    private GameObject mTooltip;
    private Camera mCam;
    private LayerMask mLayerMask;
    private float mTooltipTime;
    private bool mIsEditor;

    private bool handlesEvents
    {
        get { return eventHandler == this; }
    }

    public Camera cachedCamera
    {
        get
        {
            if (mCam == null)
            {
                mCam = GetComponent<Camera>();
            }
            return mCam;
        }
    }

    public static GameObject hoveredObject
    {
        get { return mMouse[0].current; }
    }

    public static GameObject selectedObject
    {
        get { return mSel; }
        set
        {
            if (mSel == value) return;
            if (mSel != null)
            {
                UICamera cam = FindCameraForLayer(mSel.layer);
                if (cam != null)
                {
                    current = cam;
                    currentCamera = cam.mCam;
                    Notify(mSel, "OnSelect", false);
                    if (cam.useController || cam.useKeyboard)
                    {
                        Highlight(mSel, false);
                    }
                    current = null;
                }
            }
            mSel = value;
            if (mSel == null) return;
            UICamera cam2 = FindCameraForLayer(mSel.layer);
            if (cam2 != null)
            {
                current = cam2;
                currentCamera = cam2.mCam;
                if (cam2.useController || cam2.useKeyboard)
                {
                    Highlight(mSel, true);
                }
                Notify(mSel, "OnSelect", true);
                current = null;
            }
        }
    }

    public static Camera mainCamera
    {
        get
        {
            UICamera cam = eventHandler;
            return (cam != null) ? cam.cachedCamera : null;
        }
    }

    public static UICamera eventHandler
    {
        get
        {
            for (int i = 0; i < mList.Count; i++)
            {
                UICamera cam = mList[i];
                if (cam != null && cam.enabled && NGUITools.GetActive(cam.gameObject))
                {
                    return cam;
                }
            }
            return null;
        }
    }

    private void OnApplicationQuit()
    {
        mHighlighted.Clear();
    }

    private static int CompareFunc(UICamera a, UICamera b)
    {
        if (a.cachedCamera.depth < b.cachedCamera.depth) return 1;
        if (a.cachedCamera.depth > b.cachedCamera.depth) return -1;
        return 0;
    }

    private static bool Raycast(Vector3 inPos, ref RaycastHit hit)
    {
        for (int i = 0; i < mList.Count; i++)
        {
            UICamera cam = mList[i];
            if (!cam.enabled || !NGUITools.GetActive(cam.gameObject))
                continue;

            currentCamera = cam.cachedCamera;
            Vector3 viewPos = currentCamera.ScreenToViewportPoint(inPos);
            if (viewPos.x < 0f || viewPos.x > 1f || viewPos.y < 0f || viewPos.y > 1f)
                continue;

            Ray ray = currentCamera.ScreenPointToRay(inPos);
            int layerMask = currentCamera.cullingMask & (int)cam.eventReceiverMask;
            float distance = (!(cam.rangeDistance > 0f)) ? (currentCamera.farClipPlane - currentCamera.nearClipPlane) : cam.rangeDistance;
            
            if (cam.clipRaycasts)
            {
                RaycastHit[] hits = Physics.RaycastAll(ray, distance, layerMask);
                if (hits.Length > 1)
                {
                    Array.Sort(hits, (RaycastHit r1, RaycastHit r2) => r1.distance.CompareTo(r2.distance));
                    for (int j = 0; j < hits.Length; j++)
                    {
                        if (IsVisible(ref hits[j]))
                        {
                            hit = hits[j];
                            return true;
                        }
                    }
                    return false;
                }
                if (hits.Length == 1 && IsVisible(ref hits[0]))
                {
                    hit = hits[0];
                    return true;
                }
            }
            if (Physics.Raycast(ray, out hit, distance, layerMask))
                return true;
        }
        return false;
    }

    private static bool IsVisible(ref RaycastHit hit)
    {
        UIPanel panel = NGUITools.FindInParents<UIPanel>(hit.collider.gameObject);
        if (panel == null || panel.clipping == UIDrawCall.Clipping.None || panel.IsVisible(hit.point))
            return true;
        return false;
    }

    public static UICamera FindCameraForLayer(int layer)
    {
        int mask = 1 << layer;
        for (int i = 0; i < mList.Count; i++)
        {
            UICamera cam = mList[i];
            Camera c = cam.cachedCamera;
            if (c != null && (c.cullingMask & mask) != 0)
                return cam;
        }
        return null;
    }

    private static int GetDirection(KeyCode up, KeyCode down)
    {
        if (Input.GetKeyDown(up))
            return 1;
        if (Input.GetKeyDown(down))
            return -1;
        return 0;
    }

    private static int GetDirection(KeyCode up0, KeyCode up1, KeyCode down0, KeyCode down1)
    {
        if (Input.GetKeyDown(up0) || Input.GetKeyDown(up1))
            return 1;
        if (Input.GetKeyDown(down0) || Input.GetKeyDown(down1))
            return -1;
        return 0;
    }

    private static int GetDirection(string axis)
    {
        float now = Time.realtimeSinceStartup;
        if (mNextEvent < now)
        {
            float axisVal = Input.GetAxis(axis);
            if (axisVal > 0.75f)
            {
                mNextEvent = now + 0.25f;
                return 1;
            }
            if (axisVal < -0.75f)
            {
                mNextEvent = now + 0.25f;
                return -1;
            }
        }
        return 0;
    }

    public static bool IsHighlighted(GameObject go)
    {
        int count = mHighlighted.Count;
        while (count > 0)
        {
            Highlighted hl = mHighlighted[--count];
            if (hl.go == go)
                return true;
        }
        return false;
    }

    private static void Highlight(GameObject go, bool highlight)
    {
        if (go == null)
            return;
        int count = mHighlighted.Count;
        while (count > 0)
        {
            Highlighted hl = mHighlighted[--count];
            if (hl == null || hl.go == null)
                mHighlighted.RemoveAt(count);
            else if (hl.go == go)
            {
                if (highlight)
                    hl.counter++;
                else if (--hl.counter < 1)
                {
                    mHighlighted.Remove(hl);
                    Notify(go, "OnHover", false);
                }
                return;
            }
        }
        if (highlight)
        {
            Highlighted newHl = new Highlighted();
            newHl.go = go;
            newHl.counter = 1;
            mHighlighted.Add(newHl);
            Notify(go, "OnHover", true);
        }
    }

    private static void Notify(GameObject go, string funcName, object obj)
    {
        if (go != null)
        {
            go.SendMessage(funcName, obj, SendMessageOptions.DontRequireReceiver);
            if (genericEventHandler != null && genericEventHandler != go)
            {
                genericEventHandler.SendMessage(funcName, obj, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private MouseOrTouch GetTouch(int id)
    {
        MouseOrTouch touch;
        if (!mTouches.TryGetValue(id, out touch))
        {
            touch = new MouseOrTouch();
            touch.touchBegan = true;
            mTouches.Add(id, touch);
        }
        return touch;
    }

    private void RemoveTouch(int id)
    {
        mTouches.Remove(id);
    }

    // =========================================================================
    // Initialization and Update Methods
    // =========================================================================

    private void Awake()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            useMouse = false;
            useTouch = true;
            useKeyboard = false;
            useController = false;
        }
        else if (Application.platform == RuntimePlatform.PS3 || Application.platform == RuntimePlatform.XBOX360)
        {
            useMouse = false;
            useTouch = false;
            useKeyboard = false;
            useController = true;
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
        {
            mIsEditor = true;
        }
        mMouse[0].pos.x = Input.mousePosition.x;
        mMouse[0].pos.y = Input.mousePosition.y;
        lastTouchPosition = mMouse[0].pos;
        mList.Add(this);
        mList.Sort(CompareFunc);
        if ((int)eventReceiverMask == -1)
        {
            eventReceiverMask = GetComponent<Camera>().cullingMask;
        }
    }

    private void OnDestroy()
    {
        mList.Remove(this);
    }

    private void FixedUpdate()
    {
        if (useMouse && Application.isPlaying && handlesEvents)
        {
            GameObject go = (!Raycast(Input.mousePosition, ref lastHit)) ? fallThrough : lastHit.collider.gameObject;
            if (go == null)
            {
                go = genericEventHandler;
            }
            for (int i = 0; i < 3; i++)
            {
                mMouse[i].current = go;
            }
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2) && gameObject.name == "Camera")
        {
            if (cachedCamera.depth == 1)
                cachedCamera.depth = -2;
            else
                cachedCamera.depth = 1;
        }

        if (!Application.isPlaying || !handlesEvents)
        {
            return;
        }
        current = this;
        if (useMouse || (useTouch && mIsEditor))
        {
            ProcessMouse();
        }
        if (useTouch)
        {
            ProcessTouches();
        }
        if (useMouse && mSel != null &&
            ((cancelKey0 != KeyCode.None && Input.GetKeyDown(cancelKey0)) || (cancelKey1 != KeyCode.None && Input.GetKeyDown(cancelKey1))))
        {
            selectedObject = null;
        }
        if (mSel != null)
        {
            string text = Input.inputString;
            if (useKeyboard && Input.GetKeyDown(KeyCode.Delete))
            {
                text += "\b";
            }
            if (text.Length > 0)
            {
                if (!stickyTooltip && mTooltip != null)
                {
                    ShowTooltip(false);
                }
                Notify(mSel, "OnInput", text);
            }
            ProcessOthers();
        }
        else
        {
            inputHasFocus = false;
        }
        if (useMouse && mHover != null)
        {
            float axis = Input.GetAxis(scrollAxisName);
            if (axis != 0f)
            {
                Notify(mHover, "OnScroll", axis);
            }
            if (showTooltips && mTooltipTime != 0f &&
                (mTooltipTime < Time.realtimeSinceStartup || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                mTooltip = mHover;
                ShowTooltip(true);
            }
        }
        current = null;
    }

    private void ProcessMouse()
    {
        bool flag = useMouse && Time.timeScale < 0.9f;
        if (!flag)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Input.GetMouseButton(i) || Input.GetMouseButtonUp(i))
                {
                    flag = true;
                    break;
                }
            }
        }
        mMouse[0].pos = Input.mousePosition;
        mMouse[0].delta = mMouse[0].pos - lastTouchPosition;
        bool flag2 = mMouse[0].pos != lastTouchPosition;
        lastTouchPosition = mMouse[0].pos;
        if (flag)
        {
            GameObject go = (!Raycast(Input.mousePosition, ref lastHit)) ? fallThrough : lastHit.collider.gameObject;
            if (go == null)
            {
                go = genericEventHandler;
            }
            mMouse[0].current = go;
        }
        for (int j = 1; j < 3; j++)
        {
            mMouse[j].pos = mMouse[0].pos;
            mMouse[j].delta = mMouse[0].delta;
            mMouse[j].current = mMouse[0].current;
        }
        bool flag3 = false;
        for (int k = 0; k < 3; k++)
        {
            if (Input.GetMouseButton(k))
            {
                flag3 = true;
                break;
            }
        }
        if (flag3)
            mTooltipTime = 0f;
        else if (useMouse && flag2 && (!stickyTooltip || mHover != mMouse[0].current))
        {
            if (mTooltipTime != 0f)
                mTooltipTime = Time.realtimeSinceStartup + tooltipDelay;
            else if (mTooltip != null)
                ShowTooltip(false);
        }
        if (useMouse && !flag3 && mHover != mMouse[0].current)
        {
            if (mTooltip != null)
                ShowTooltip(false);
            Highlight(mHover, false);
            mHover = null;
        }
        if (useMouse)
        {
            for (int l = 0; l < 3; l++)
            {
                bool mouseButtonDown = Input.GetMouseButtonDown(l);
                bool mouseButtonUp = Input.GetMouseButtonUp(l);
                currentTouch = mMouse[l];
                currentTouchID = -1 - l;
                if (mouseButtonDown)
                {
                    currentTouch.pressedCam = currentCamera;
                }
                else if (currentTouch.pressed != null)
                {
                    currentCamera = currentTouch.pressedCam;
                }
                ProcessTouch(mouseButtonDown, mouseButtonUp);
            }
            currentTouch = null;
        }
        if (useMouse && !flag3 && mHover != mMouse[0].current)
        {
            mTooltipTime = Time.realtimeSinceStartup + tooltipDelay;
            mHover = mMouse[0].current;
            Highlight(mHover, true);
        }
    }

    private void ProcessTouches()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (allowMultiTouch || touch.fingerId == 0)
            {
                currentTouchID = (!allowMultiTouch) ? 1 : touch.fingerId;
                currentTouch = GetTouch(currentTouchID);
                bool flag = (touch.phase == TouchPhase.Began) || currentTouch.touchBegan;
                bool flag2 = (touch.phase == TouchPhase.Canceled) || (touch.phase == TouchPhase.Ended);
                currentTouch.touchBegan = false;
                if (flag)
                {
                    currentTouch.delta = Vector2.zero;
                }
                else
                {
                    currentTouch.delta = touch.position - currentTouch.pos;
                }
                currentTouch.pos = touch.position;
                currentTouch.current = (!Raycast(currentTouch.pos, ref lastHit)) ? fallThrough : lastHit.collider.gameObject;
                if (currentTouch.current == null)
                    currentTouch.current = genericEventHandler;
                lastTouchPosition = currentTouch.pos;
                if (flag)
                {
                    currentTouch.pressedCam = currentCamera;
                }
                else if (currentTouch.pressed != null)
                {
                    currentCamera = currentTouch.pressedCam;
                }
                ProcessTouch(flag, flag2);
                if (flag2)
                    RemoveTouch(currentTouchID);
                currentTouch = null;
            }
        }
    }

    private void ProcessOthers()
    {
        currentTouchID = -100;
        currentTouch = mController;
        inputHasFocus = (mSel != null && mSel.GetComponent<UIInput>() != null);
        bool flag = ((submitKey0 != KeyCode.None && Input.GetKeyDown(submitKey0)) || (submitKey1 != KeyCode.None && Input.GetKeyDown(submitKey1)));
        bool flag2 = ((submitKey0 != KeyCode.None && Input.GetKeyUp(submitKey0)) || (submitKey1 != KeyCode.None && Input.GetKeyUp(submitKey1)));
        if (flag || flag2)
        {
            currentTouch.current = mSel;
            ProcessTouch(flag, flag2);
        }
        int num = 0;
        int num2 = 0;
        if (useKeyboard)
        {
            if (mSel != null && mSel.GetComponent<UIInput>() != null)
            {
                num += GetDirection(KeyCode.UpArrow, KeyCode.DownArrow);
                num2 += GetDirection(KeyCode.RightArrow, KeyCode.LeftArrow);
            }
            else
            {
                num += GetDirection(KeyCode.W, KeyCode.UpArrow, KeyCode.S, KeyCode.DownArrow);
                num2 += GetDirection(KeyCode.D, KeyCode.RightArrow, KeyCode.A, KeyCode.LeftArrow);
            }
        }
        if (useController)
        {
            if (!string.IsNullOrEmpty(verticalAxisName))
                num += GetDirection(verticalAxisName);
            if (!string.IsNullOrEmpty(horizontalAxisName))
                num2 += GetDirection(horizontalAxisName);
        }
        if (num != 0)
            Notify(mSel, "OnKey", (num <= 0) ? KeyCode.DownArrow : KeyCode.UpArrow);
        if (num2 != 0)
            Notify(mSel, "OnKey", (num2 <= 0) ? KeyCode.LeftArrow : KeyCode.RightArrow);
        if (useKeyboard && Input.GetKeyDown(KeyCode.Tab))
            Notify(mSel, "OnKey", KeyCode.Tab);
        if (cancelKey0 != KeyCode.None && Input.GetKeyDown(cancelKey0))
            Notify(mSel, "OnKey", KeyCode.Escape);
        if (cancelKey1 != KeyCode.None && Input.GetKeyDown(cancelKey1))
            Notify(mSel, "OnKey", KeyCode.Escape);
        currentTouch = null;
    }

    private void ProcessTouch(bool pressed, bool unpressed)
    {
        bool flag = currentTouch == mMouse[0];
        float num = (!flag) ? touchDragThreshold : mouseDragThreshold;
        float num2 = (!flag) ? Mathf.Max(touchClickThreshold, Screen.height * 0.1f) : mouseClickThreshold;

        if (pressed)
        {
            if (mTooltip != null)
            {
                ShowTooltip(false);
            }
            currentTouch.pressed = currentTouch.current;
            currentTouch.clickNotification = ClickNotification.Always;
            currentTouch.totalDelta = Vector2.zero;
            currentTouch.dragStarted = false;
            Notify(currentTouch.pressed, "OnPress", true);
            if (currentTouch.pressed != mSel)
            {
                if (mTooltip != null)
                {
                    ShowTooltip(false);
                }
                selectedObject = null;
            }
        }
        else if (currentTouch.pressed != null)
        {
            float magnitude = currentTouch.delta.magnitude;
            if (magnitude != 0f)
            {
                currentTouch.totalDelta += currentTouch.delta;
                magnitude = currentTouch.totalDelta.magnitude;
                if (!currentTouch.dragStarted && num < magnitude)
                {
                    currentTouch.dragStarted = true;
                    currentTouch.delta = currentTouch.totalDelta.normalized;
                }
                if (currentTouch.dragStarted)
                {
                    if (mTooltip != null)
                    {
                        ShowTooltip(false);
                    }
                    bool flag2 = (currentTouch.clickNotification == ClickNotification.None);
                    Notify(currentTouch.pressed, "OnDrag", currentTouch.delta);
                    if (flag2)
                    {
                        currentTouch.clickNotification = ClickNotification.None;
                    }
                    else if (currentTouch.clickNotification == ClickNotification.BasedOnDelta && num2 < magnitude)
                    {
                        currentTouch.clickNotification = ClickNotification.None;
                    }
                }
            }
        }
        
        if (!unpressed)
        {
            return;
        }

        if (mTooltip != null)
        {
            ShowTooltip(false);
        }

        if (currentTouch.pressed != null)
        {
            Notify(currentTouch.pressed, "OnPress", false);
            if (useMouse && currentTouch.pressed == mHover)
            {
                Notify(currentTouch.pressed, "OnHover", true);
            }
            if (currentTouch.pressed == currentTouch.current ||
               (currentTouch.clickNotification != ClickNotification.None && currentTouch.totalDelta.magnitude < num))
            {
                if (currentTouch.pressed != mSel)
                {
                    mSel = currentTouch.pressed;
                    Notify(currentTouch.pressed, "OnSelect", true);
                }
                else
                {
                    mSel = currentTouch.pressed;
                }
                if (currentTouch.clickNotification != ClickNotification.None)
                {
                    float realtimeSinceStartup = Time.realtimeSinceStartup;
                    Notify(currentTouch.pressed, "OnClick", null);
                    if (currentTouch.clickTime + 0.25f > realtimeSinceStartup)
                    {
                        Notify(currentTouch.pressed, "OnDoubleClick", null);
                    }
                    currentTouch.clickTime = realtimeSinceStartup;
                }
            }
            else
            {
                Notify(currentTouch.current, "OnDrop", currentTouch.pressed);
            }
        }
        currentTouch.dragStarted = false;
        currentTouch.pressed = null;
    }

    public void ShowTooltip(bool val)
    {
        mTooltipTime = 0f;
        Notify(mTooltip, "OnTooltip", val);
        if (!val)
        {
            mTooltip = null;
        }
    }
}

