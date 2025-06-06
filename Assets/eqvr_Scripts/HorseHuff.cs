using Aaron_25;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class HorseHuff : MonoBehaviour
{
    public float increment = 10f;
    private float timer;


    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= increment)
        {
            Debug.Log("Playing Horse Huff Sound");
            var index  = UnityEngine.Random.Range(0, 3);
            Debug.Log("Index is : " + index);
            AudioManager.Instance.PlayAudio(index, AudioLibraryType.Horse);
            timer = 0f; // Reset the timer after playing the sound
        }
    }
}