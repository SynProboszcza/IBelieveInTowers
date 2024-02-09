using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float health = 100f;
    //public ParticleSystem particle;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(health < 0)
        {
            //bool isEmitted = false;
            //if (!isEmitted)
            //{
            //    GameObject go = Instantiate(particle.gameObject);
            //    go.GetComponent<ParticleSystem>().Play();
            //    Destroy(go, 3f);
            //    isEmitted = true;
            //}
            Destroy(gameObject);
        }
    }

    public void takeDamage(float damage)
    {
        this.health -= damage;
    }

    public void setHealth(float health)
    {
        this.health = health;
    }

    public float getHealth()
    {
        return this.health;
    }
}
