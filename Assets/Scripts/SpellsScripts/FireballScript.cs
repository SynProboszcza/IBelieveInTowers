using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballScript : MonoBehaviour
{
    public float spellRadius = 5f; // Adjust this value in the Inspector to set the spell's radius

    void CastFireball(Vector3 fireballPosition)
    {
        DetectUnitsWithinRadius(fireballPosition, spellRadius);
    }

    void DetectUnitsWithinRadius(Vector3 fireballPosition, float radius)
    {
        // Use OverlapSphere to detect colliders within the specified radius
        Collider[] colliders = Physics.OverlapSphere(fireballPosition, radius);

        foreach (Collider collider in colliders)
        {
            // Check if the collider has a UnitStatistics component
            UnitStatistics unitStats = collider.GetComponent<UnitStatistics>();

            if (unitStats != null)
            {
                // This collider represents a unit within the fireball's radius
                // Apply spell effects (in this case, decrease health by 20%)
                ApplySpellEffects(unitStats);

                // Update unit status after applying the spell effects
                UpdateUnitStatus(unitStats);
            }
        }
    }

    void ApplySpellEffects(UnitStatistics unitStats)
    {
        // Apply spell effects to the detected unit
        // For example, decrease health by 20%
        unitStats.TakeDamage(unitStats.maxHealth * 0.2f); // Reducing health by 20%
        Debug.Log("Applied spell effects to: " + unitStats.gameObject.name + " - Health decreased by 20%");
    }

    void UpdateUnitStatus(UnitStatistics unitStats)
    {
        // Update unit status based on the applied spell effects
        // For example, check if the unit has died
        if (unitStats.maxHealth <= 0)
        {
            Destroy(unitStats.gameObject); // Destroy the GameObject when health drops below or equals 0
            Debug.Log(unitStats.gameObject.name + " has been defeated!");
            // Add other status updates or UI changes as needed
        }
    }
}