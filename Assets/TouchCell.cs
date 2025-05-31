using System;
using UnityEngine;

public class TouchCell : MonoBehaviour {
  [SerializeField] private int btn;
  public GameObject player;

  void Start() {
    player = GameManager.Instance.GetPlayer();
  }
  private void OnCollisionEnter(Collision other) {
    if (other.gameObject.CompareTag("Cell")) return;
    Debug.Log("Collision");
    GetComponent<Renderer>().material.color = Color.red;
    SendBtn();
  }

  private void OnCollisionExit(Collision other) {
    if (other.gameObject.CompareTag("Cell")) return;
    Debug.Log("Collision left");
    GetComponent<Renderer>().material.color = Color.white;
    ReleaseButton();
  }

  [ContextMenu("Test")]
  public void SendBtn() {
    ChuniIO.Instance.SendButtonToIO(btn);
  }

  public void ReleaseButton() {
    ChuniIO.Instance.ReleaseButtonFromIO(btn);
  }
}
