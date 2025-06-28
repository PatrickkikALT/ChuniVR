using UnityEngine;
/// <summary>
/// Helper class that makes sure IR sensors never extend beyond the other side.
/// </summary>
public class IRSensor : MonoBehaviour {
  public Transform target;
  public float distance;
  
  private void Start() {
    distance = GetDistance(gameObject, target.gameObject);
  }
  
  private float GetDistance(GameObject objectA, GameObject objectB) {
    Vector3 start = objectA.transform.position;
    Vector3 end = objectB.transform.position;
    float dis = Vector3.Distance(start, end);

    return dis;
  }
}
