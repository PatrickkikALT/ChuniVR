using System;
using UnityEngine;

public class IRSensor : MonoBehaviour {
  public Transform target;
  public float distance;
  
  private void Start() {
    distance = GetDistance(gameObject, target.gameObject);
  }
  
  private float GetDistance(GameObject objectA, GameObject objectB) {
    Vector3 start = objectA.transform.position;
    Vector3 end = objectB.transform.position;

    Vector3 direction = (end - start).normalized;
    float dis = Vector3.Distance(start, end);

    return dis;
  }
}
