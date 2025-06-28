using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class EAmusementGenerator : MonoBehaviour
{
  [SerializeField] private GameObject card;
  private Transform cardSpawn;
  [SerializeField] private TMP_InputField aimeIDInput;

  private void Start() {
    cardSpawn = transform.GetChild(0);
  }

  public void GenerateCard() {
    var c = Instantiate(card, cardSpawn.position, transform.rotation);
    c.GetComponent<EAmusementCard>().id = aimeIDInput.text.Length != 20 ? GenerateRandomAimeID() : aimeIDInput.text;
  }
  
  public static string GenerateRandomAimeID() {
    char[] digits = new char[20];
    digits[0] = (char)('1' + Random.Range(0, 9));
    for (int i = 1; i < 20; i++) {
      digits[i] = (char)('0' + Random.Range(0, 10));
    }

    return new string(digits);
  }
}
