using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;


public class TouchCell : MonoBehaviour {
  public int btn;
  public GameObject player;

  void Start() {
    player = GameManager.Instance.GetPlayer();
  }
  private void OnCollisionEnter(Collision other) {
    if (other.gameObject.layer != 6) return;
    if (other.transform.parent.parent.TryGetComponent(out HapticImpulsePlayer haptic)) 
      SendHaptic(haptic);
    GetComponent<Renderer>().material.color = Color.red;
    SendBtn();
  }

  private void OnCollisionStay(Collision other) {
    if (other.gameObject.layer != 6) return; 
    if (other.transform.parent.parent.TryGetComponent(out HapticImpulsePlayer haptic)) 
      SendHaptic(haptic);
  }

  private void OnCollisionExit(Collision other) {
    if (other.gameObject.layer != 6) return;
    GetComponent<Renderer>().material.color = Color.white;

    ReleaseButton();
  }

  private bool SendHaptic(HapticImpulsePlayer haptic) 
    => haptic.SendHapticImpulse(0.6f, 0.1f);
  
  [ContextMenu("Test")]
  public void SendBtn() {
    ChuniIO.Instance.SendButtonToIO(btn);
  }

  public void ReleaseButton() {
    ChuniIO.Instance.ReleaseButtonFromIO(btn);
  }
}
