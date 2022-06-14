using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniRx;
using InputObservable;

public static class Vector3Extension {
    public static Vector3 X(this Vector3 vec, float v) {
        return new Vector3(v, vec.y, vec.z);
    }

    public static Vector3 Y(this Vector3 vec, float v)
    {
        return new Vector3(vec.x, v, vec.z);
    }

    public static Vector3 Z(this Vector3 vec, float v)
    {
        return new Vector3(vec.x, vec.y, v);
    }
}

public class TPSController : MonoBehaviour
{
    [SerializeField]
    Transform player;

    [SerializeField]
    Player playerObject;


    [SerializeField]
    new Transform camera;

    [SerializeField]
    Transform mazzle;

    [SerializeField]
    Vector3 cameraOffset = new Vector3(0, 1, 3);

    [SerializeField]
    Joystick moveJoystick;

    [SerializeField]
    float zdiff = 0.1f;

    [SerializeField]
    float rdiff = 1.2f;

    [SerializeField]
    Button shotButton;

    [SerializeField]
    GameObject bulletPrefab;

    [SerializeField]
    float bulletSpeed = 50;

    [SerializeField]
    GameObject partilePrefab;

    Vector3 targetPosition = Vector3.zero;

    void diffToCameraMove(Vector2 diff, float hratio, float vratio)
    {
        var e = diff.ToEulerAngle(hratio, vratio);
        camera.RotateAround(player.position, Vector3.up, -e.y);
        camera.Rotate(-e.x, 0, 0);

        // mazzle.RotateAround(player.position, Vector3.right, -e.x);
    }

    void ignoreCollider(GameObject bullet) {
        var collider = bullet.GetComponent<Collider>();
        playerObject.IgnoreCollider(collider);
    }

    void shot() {
        // 
        LookForward();
        MazzleDirection();

        var go = Instantiate(bulletPrefab);
        ignoreCollider(go);
        go.transform.position = mazzle.position;
        var rb = go.GetComponent<Rigidbody>();
        rb.velocity = mazzle.forward.normalized * bulletSpeed;
        // rb.AddForce(camera.forward * bulletForce, ForceMode.Impulse);
        var bullet = go.GetComponent<Bullet>();
        bullet.subject.Subscribe(point => {
            var go = Instantiate(partilePrefab);
            go.transform.position = point;
            go.transform.LookAt(camera.position);
            // Debug.Log($"{contact.point}, {contact.normal}");
            // Debug.Break();
            var ps = go.GetComponent<ParticleSystem>();
            ps.Play();
        }).AddTo(this);
    }

    void Start() {
        var context = this.DefaultInputContext(EventSystem.current);
        var hratio = -180.0f / Screen.width;
        var vratio = -120.0f / Screen.height;
        context.GetObservable(0).Difference().Subscribe(diff => diffToCameraMove(diff, hratio, vratio)).AddTo(this);
        context.GetObservable(1).Difference().Subscribe(diff => diffToCameraMove(diff, hratio, vratio)).AddTo(this);
        shotButton.onClick.AsObservable().Subscribe(_ => shot()).AddTo(this);
    }

    void WalkPlayerByJoystick()
    {
        player.Translate(Vector3.forward * moveJoystick.Vertical * zdiff + Vector3.right * moveJoystick.Horizontal * zdiff);
    }

    void CameraChasePlayer()
    {
        // camera.position = player.position + offset;
        // var dist = offset.normalized;
        // camera.position = player.position + camera.forward * dist.z;
        // camera.position = camera.TransformDirection(player.position + offset);

        // camera.position = player.position + camera.forward * (-3) + Vector3.up * offset.y;
        // camera.position = player.position + camera.forward * offset.z + Vector3.up * offset.y;

        var forward = camera.forward.Y(0);
        camera.position = player.position + forward * (-cameraOffset.z) + Vector3.up * cameraOffset.y;
    }

    void LookForward()
    {
        player.LookAt(player.position + camera.forward.Y(0));
    }

    void SyncPlayerDirection()
    {
        if (Mathf.Abs(moveJoystick.Vertical) >= 0.4 || Mathf.Abs(moveJoystick.Horizontal) > 0.4)
        {
            LookForward();
        }
    }

    void MazzleDirection()
    {
        var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 100));
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 100)) {
            targetPosition = hit.point;
        } else
        {
            targetPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 100));
        }
        // Camera.main.ScreenToViewportPoint
        mazzle.LookAt(targetPosition);
    }

    void Update()
    {
        WalkPlayerByJoystick();
        CameraChasePlayer();
        SyncPlayerDirection();
        MazzleDirection();

        WalkPlayerByArrawKey();

        Debug.DrawLine(player.position, player.position + player.forward * 100, Color.red);
        Debug.DrawLine(camera.position, camera.position + camera.forward * 100, Color.green);
        Debug.DrawLine(player.position, player.position + camera.forward * 100, Color.yellow);
        // Debug.DrawLine(mazzle.position, mazzle.position + mazzle.forward * 100, Color.blue);
        Debug.DrawLine(mazzle.position, targetPosition, Color.blue);
    }

    void WalkPlayerByArrawKey()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            player.Translate(Vector3.forward * zdiff);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            player.Translate(Vector3.forward * -zdiff);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                player.Translate(Vector3.right * zdiff);
            }
            else
            {
                player.Rotate(Vector3.up * rdiff);
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                player.Translate(Vector3.right * -zdiff);
            }
            else
            {
                player.Rotate(Vector3.up * -rdiff);
            }
        }
    }
}
