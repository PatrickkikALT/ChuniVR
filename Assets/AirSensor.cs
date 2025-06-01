using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class AirSensor : MonoBehaviour {

  private ChuniIO IO;
  public IRSensor[] irSensors;
  public Vector3[] beamDirections;
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
      Vector3 direction = beamDirections[i];
      float distance = irSensors[i].distance;

      bool isBroken = Physics.Raycast(origin, direction, distance);
      if (isBroken) {
        localState |= (1 << i);
      }
    }

    lock (beamLock) {
      beamState = localState;
    }
  }

  private void AirLoop() {
    while (IO.running) {
      int currentState;
      lock (beamLock) {
        currentState = beamState;
      }

      IO.beams = (byte)currentState;
      Thread.Sleep(1);
    }
  }

}
