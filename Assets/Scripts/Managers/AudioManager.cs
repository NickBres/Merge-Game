using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private AudioSource mergeSource;
    


    void Awake()
    {
        MergeManager.onMergeAnimal += PlayMergeSound;
    }

    public void PlayMergeSound(AnimalType animalType, Vector2 position)
    {
        Debug.Log("Play Merge Sound");
        mergeSource.pitch = Random.Range(0.8f, 1.2f);
        mergeSource.Play();
    }
}
