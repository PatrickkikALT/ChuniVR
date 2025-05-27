using System;
using UnityEngine;

public class GameManager : MonoBehaviour {
  public static GameManager Instance;
  
  [SerializeField] private GameObject player;
  public GameObject GetPlayer() => player;

  private void Awake() {
    if (!Instance) Instance = this; else Destroy(this);
  }
}
