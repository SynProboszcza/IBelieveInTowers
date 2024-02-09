using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ExplosionAnimation : MonoBehaviour
{
    public float timeToDestruction = 0.25f;
    public float timeToDelayAnimation = 0.1f;

    private void Start()
    {
        StartCoroutine(DelayThenAnimate(timeToDelayAnimation, timeToDestruction));
    }

    IEnumerator Animation(float time)
    {
        for (float i = time; i >= 0f; i -= Time.deltaTime)
        {
            float scale = Mathf.InverseLerp(0, time, i) * 2;
            transform.localScale = new Vector3(scale, scale, scale);
            yield return null;// new WaitForSeconds(0.1f);
        }
    }

    IEnumerator DelayThenAnimate(float delayTime, float animTime)
    {
        yield return new WaitForSeconds(delayTime);
        StartCoroutine(Animation(animTime));
    }
}
