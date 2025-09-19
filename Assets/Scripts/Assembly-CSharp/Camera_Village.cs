using System;
using UnityEngine;

public class Camera_Village : MonoBehaviour
{
    public float rotation_damping = 0.5f;
    public float angle_min = 125f;
    public float angle_max = 195f;
    public float border_min = 120f;
    public float border_max = 200f;

    private float target_angle;
    private bool open_closer;
    private Vector3 target_position;
    private float closer_angle;
    private bool open_scroll = true;
    private float closer_move_speed = 0.8f;
    private float closer_angle_speed = 2f;

    public GameObject toggleTarget; // 👈 Assign this in Inspector

    private void Start()
    {
        target_angle = transform.rotation.eulerAngles.y;
        closer_angle = transform.rotation.eulerAngles.y;
    }

    private void LateUpdate()
    {
        UpdateMove();
        UpdateCloser();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ToggleTargetObject();
        }
    }

    public void DoBegin()
    {
        target_angle = transform.rotation.eulerAngles.y;
        open_scroll = false;
    }

    public void DoMoveBegin()
    {
        open_scroll = false;
    }

    public void DoMove(float wapram)
    {
        float num = wapram / 400f;
        num = Mathf.Clamp(num, -1f, 1f);
        float num2 = Mathf.Asin(num) * 2f * (180f / (float)Math.PI);
        if (transform.eulerAngles.y > angle_max || transform.eulerAngles.y < angle_min)
        {
            num2 *= 0.25f;
        }
        target_angle -= num2;
        if (target_angle < border_min)
        {
            target_angle = border_min;
        }
        else if (target_angle > border_max)
        {
            target_angle = border_max;
        }
    }

    public void DoMoveEnd()
    {
        open_scroll = true;
    }

    private void UpdateMove()
    {
        float y = transform.eulerAngles.y;
        y = Mathf.LerpAngle(y, target_angle, rotation_damping * 8f * Time.deltaTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, y, transform.eulerAngles.z);

        if (open_scroll)
        {
            if (transform.eulerAngles.y < angle_min)
            {
                target_angle = angle_min;
            }
            else if (transform.eulerAngles.y > angle_max)
            {
                target_angle = angle_max;
            }
        }
    }

    public void SetCloser(Transform go_target)
    {
        if (!open_closer)
        {
            open_closer = true;
            target_position = go_target.position;
            closer_angle = Quaternion.FromToRotation(transform.forward, target_position - transform.position).eulerAngles.y;
            if (closer_angle > 180f)
            {
                closer_angle -= 360f;
            }
            closer_angle += transform.eulerAngles.y;
        }
    }

    private void UpdateCloser()
    {
        if (open_closer)
        {
            Vector3 position = transform.position;
            position = Vector3.Lerp(position, target_position, closer_move_speed * Time.deltaTime);
            transform.position = position;

            float y = transform.eulerAngles.y;
            y = Mathf.Lerp(y, closer_angle, closer_angle_speed * Time.deltaTime);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, y, transform.eulerAngles.z);
        }
    }

    public void SetCurrentAngle(Vector3 m_angle)
    {
        transform.eulerAngles = m_angle;
        if (transform.eulerAngles.y < angle_min)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, angle_min, transform.eulerAngles.z);
        }
        if (transform.eulerAngles.y > angle_max)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, angle_max, transform.eulerAngles.z);
        }
        target_angle = transform.eulerAngles.y;
    }

    public Vector3 GetCurrentAngle()
    {
        return transform.eulerAngles;
    }

    public float GetPersentAngle()
    {
        if (angle_max == angle_min)
        {
            Debug.Log("error!");
            return 0f;
        }
        return (transform.eulerAngles.y - angle_min) / (angle_max - angle_min);
    }

    private void ToggleTargetObject()
    {
        if (toggleTarget != null)
        {
            bool isActive = toggleTarget.activeSelf;
            toggleTarget.SetActive(!isActive);
        }
    }
}
