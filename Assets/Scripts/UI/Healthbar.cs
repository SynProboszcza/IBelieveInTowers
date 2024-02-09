using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    private float objectHealth;
    private float objectMaxHealth;
    private Transform healthbar;
    public Image fill;
    public Vector3 healthbarOffset = new Vector3(0,-0.5f,0);

    private void Start()
    {
        healthbar = gameObject.transform.Find("Healthbar");
    }

    void Update()
    {
        if (gameObject.transform.parent.TryGetComponent<Enemy>(out Enemy enemyParent))
        {
            // Set healthbar position according to parent object position
            healthbar.position = transform.parent.position + healthbarOffset;
            // Find current and max health and fill properly
            objectHealth = enemyParent.GetHealth();
            objectMaxHealth = enemyParent.GetMaxHealth();
        } else if (gameObject.transform.parent.TryGetComponent<MainTurret>(out MainTurret turretParent))
        {
            // Set healthbar position according to parent object position
            healthbar.position = transform.parent.position + healthbarOffset;
            // Find current and max health and fill properly
            objectHealth = turretParent.GetHealth();
            objectMaxHealth = turretParent.GetMaxHealth();
        } else if (gameObject.transform.parent.TryGetComponent<MultiplayerEnemy>(out MultiplayerEnemy mEnemyParent))
        {
            // Set healthbar position according to parent object position
            healthbar.position = transform.parent.position + healthbarOffset;
            // Find current and max health and fill properly
            objectHealth = mEnemyParent.GetHealth();
            objectMaxHealth = mEnemyParent.GetMaxHealth();
        }
            fill.fillAmount = objectHealth / objectMaxHealth;
    }
}
