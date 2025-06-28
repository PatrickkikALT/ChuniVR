using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.OpenXR;

/// <summary>
/// Detects collisions with just controllers (layer 6) and sends corresponding input to ChuniIO.
/// Also triggers haptic feedback when appropriate
/// </summary>
public class TouchCell : MonoBehaviour {
  public int btn;
  private bool xrDeviceEnabled;
  private void Start() {
    try {
      var checkForInstance = XRDeviceSimulator.instance;
      xrDeviceEnabled = checkForInstance.enabled;
    }
    catch {
      xrDeviceEnabled = false;
    }
  }

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

  private bool SendHaptic(HapticImpulsePlayer haptic) {
    if (xrDeviceEnabled) {
      return false;
    }
    
    var con = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).name.ToLower();
    var amplitude = con.Contains("vive") ? 0.4f : 0.6f;
    
    return haptic.SendHapticImpulse(amplitude, 0.1f);
  }
  
  public void SendBtn() {
    ChuniIO.Instance.SendButtonToIO(btn);
  }

  public void ReleaseButton() {
    ChuniIO.Instance.ReleaseButtonFromIO(btn);
  }
}
