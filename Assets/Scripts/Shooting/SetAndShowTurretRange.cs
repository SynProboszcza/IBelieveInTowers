using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAndShowTurretRange : MonoBehaviour
{
    SpriteRenderer sr;
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        float range = transform.parent.gameObject.GetComponent<CircleCollider2D>().radius;
        sr.transform.localScale = new Vector3(range * 2, range * 2, range * 2);
        transform.Find("RangeCenter").gameObject.SetActive(false);
        GetComponent<SpriteRenderer>().enabled = false;

    }


}
