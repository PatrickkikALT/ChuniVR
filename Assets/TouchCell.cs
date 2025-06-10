using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;


public class TouchCell : MonoBehaviour {
  public int btn;
  
  private void OnCollisionEnter(Collision other) {
    if (other.gameObject.layer != 6) return;
    if (other.transform.parent.parent.TryGetComponent(out HapticImpulsePlayer haptic)) 
      SendHaptic(haptic);
    SendBtn();
  }

  private void OnCollisionStay(Collision other) {
    if (other.gameObject.layer != 6) return; 
    if (other.transform.parent.parent.TryGetComponent(out HapticImpulsePlayer haptic)) 
      SendHaptic(haptic);
  }

  private void OnCollisionExit(Collision other) {
    if (other.gameObject.layer != 6) return;

    ReleaseButton();
  }

  private static bool SendHaptic(HapticImpulsePlayer haptic) 
    => haptic.SendHapticImpulse(0.6f, 0.1f);
  
  [ContextMenu("Test")]
  public void SendBtn() {
    ChuniIO.Instance.SendButtonToIO(btn);
  }

  public void ReleaseButton() {
    ChuniIO.Instance.ReleaseButtonFromIO(btn);
  }
}
