using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 velocity;

    public bool IsActive => gameObject.activeSelf;

    public void Activate(Vector3 origin, float angle)
    { // メソッド名変更
        transform.position = origin;
        velocity = 9f * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));

        gameObject.SetActive(true);
    }

    public void OnUpdate()
    { // publicにしてメソッド名変更
        var dv = velocity * Time.deltaTime;
        transform.Translate(dv.x, dv.y, 0f);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void OnBecameInvisible()
    {
        Deactivate();
    }
}