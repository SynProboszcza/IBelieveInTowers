using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class does not instantiates explosion, only sets isTurretClose flag

public class ExplodeControl : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if (collision.TryGetComponent<Bullet>(out _))
        // {
        // 
        // } else
        // {
        //     print("detected something: " + collision +
        //         "\ncollision.TryGetComponent<MultiUpgradeTurret>(out _):" + collision.TryGetComponent<MultiUpgradeTurret>(out _) +
        //         "\ntransform.parent.GetComponent<MultiplayerEnemy>().canAttackTurrets:" + transform.parent.GetComponent<MultiplayerEnemy>().canAttackTurrets);
        // }
        if (transform.parent.GetComponent<MultiplayerEnemy>().canAttackTurrets && collision.TryGetComponent<MultiUpgradeTurret>(out _))
        {
            transform.parent.GetComponent<MultiplayerEnemy>().isTurretClose = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (transform.parent.GetComponent<MultiplayerEnemy>().canAttackTurrets && collision.TryGetComponent<MultiUpgradeTurret>(out _))
        {
            transform.parent.GetComponent<MultiplayerEnemy>().isTurretClose = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // if (collision.TryGetComponent<Bullet>(out _))
        // {
        // 
        // }
        // else
        // {
        //     print("something left me: " + collision);
        // }
        if (collision.TryGetComponent<MultiUpgradeTurret>(out _))
        {
            transform.parent.GetComponent<MultiplayerEnemy>().isTurretClose = false;
        }
    }
}
