using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretOffer : MonoBehaviour
{
    public GameObject turret;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        Instantiate(turret, new Vector3(transform.parent.transform.parent.position.x, transform.parent.transform.parent.position.y, 0), Quaternion.identity);
        transform.parent.transform.parent.GetComponent<ShopContainer>().CloseShop();
        //Destroy(gameObject);
    }
}
