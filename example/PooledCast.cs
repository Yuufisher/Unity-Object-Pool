using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class PooledCast : PoolableBehaviour
{
    private Rigidbody2D rb;
    private Transform t;
    private SpriteRenderer spriteRenderer;
    private float angle;
    private Vector2 dir;
    private float speed;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        t = GetComponentInParent<Transform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void Spawn()
    {
        spriteRenderer.color = Random.ColorHSV();
        Vector2 mousePositionOnScreen = Input.mousePosition;
        mousePositionOnScreen = Camera.main.ScreenToWorldPoint(mousePositionOnScreen);
            
        transform.localPosition = mousePositionOnScreen;
        angle=Random.Range(0, 360)*Mathf.Deg2Rad;
        dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        float speed = Random.Range(4f*2, 10f*1.5f);
        
        rb.linearVelocity = dir * speed;
        rb.angularVelocity = angle*speed;
        
        StartCoroutine(AutoReturn());
    }

    private IEnumerator AutoReturn()
    {
        yield return new WaitForSeconds(2f);
        PoolManager.Instance.Despawn(this);
    }

    public override void Despawn()
    {
        StopAllCoroutines();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = 0f;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}
