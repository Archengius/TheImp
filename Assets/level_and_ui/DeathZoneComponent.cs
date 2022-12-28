using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathZoneComponent : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.name == "CharPrefab") {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}