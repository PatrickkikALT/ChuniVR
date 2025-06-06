using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EAmusementGenerator : MonoBehaviour
{
  [SerializeField] private GameObject card;
  private Transform cardSpawn;
  [SerializeField] private TMP_InputField aimeIDInput;

  private void Start() {
    cardSpawn = transform.GetChild(0);
  }

  public void GenerateCard() {
    var c = Instantiate(card, transform.position, transform.rotation);
    c.GetComponent<EAmusementCard>().id = aimeIDInput.text.Length != 20 ? GenerateRandomAimeID() : aimeIDInput.text;
  }
  
  public static string GenerateRandomAimeID() {
    var random = new System.Random();
    char[] digits = new char[20];
    digits[0] = (char)('1' + random.Next(9));
    for (int i = 1; i < 20; i++) {
      digits[i] = (char)('0' + random.Next(10));
    }

    return new string(digits);
  }
}
