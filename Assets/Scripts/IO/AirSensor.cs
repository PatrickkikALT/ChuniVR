using System.Threading;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class AirSensor : MonoBehaviour {
  private ChuniIO IO;
  public IRSensor[] irSensors;
  public int beamState = 0;

  public void Start() {
    IO = ChuniIO.Instance;
    Thread airThread = new Thread(AirLoop) {
      IsBackground = true
    };
    airThread.Start();
  }

  private object beamLock = new object();

  private void Update() {
    int localState = 0;
    for (int i = 0; i < 6; i++) {
      Vector3 origin = irSensors[i].transform.position;
      Vector3 direction = -transform.forward;
      float distance = irSensors[i].distance;

      bool isBroken = Physics.Raycast(origin, direction, distance, ~0, QueryTriggerInteraction.Collide);
      //if the ir sensor is broken, it flips the correct bit to 1 telling the game its broken.
      if (isBroken) {
        localState |= 1 << i;
      }
    }

    //keep other threads from accessing it while we are working with it
    lock (beamLock) {
      beamState = localState;
    }
  }

  private void AirLoop() {
    while (IO.running) {
      int currentState;
      //keep other threads from accessing it while we are working with it
      lock (beamLock) {
        currentState = beamState;
      }

      IO.beams = (byte)currentState;
      Thread.Sleep(1);
    }
  }
}