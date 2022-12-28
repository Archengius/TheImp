using System;
using System.Collections;
using System.Collections.Generic;
using Character.Scripts.Health;
using Creature.Scripts.Health;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class HealthUiComponent : MonoBehaviour, IHealthComponentCallback {
    [SerializeField] private CreatureHealthComponent healthComponent;
    [SerializeField] private GameObject healthPointPrefab;

    // пока так, дождусь как ты объяснишь переделаю, или сам переделай или забей, нам главное хоть что-то сдать
    private int lastHealth;
    private void Update() {
        if (lastHealth != healthComponent.Health) {
            if (healthComponent.Health <= 0) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            
            foreach (Transform child in transform) {
                Destroy(child.gameObject);
            }

            for (var i = 0; i < healthComponent.Health; i++) {
                Instantiate(healthPointPrefab, transform);
            }

            lastHealth = healthComponent.Health;
        }
        
    }
}