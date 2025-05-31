using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private AudioSource mergeSource;



    void Awake()
    {
        MergeManager.onMergeAnimal += PlayMergeSound;
        OptionsManager.onSFXToggle += ToggleSFX;
    }



    void OnDestroy()
    {
        MergeManager.onMergeAnimal -= PlayMergeSound;
        OptionsManager.onSFXToggle -= ToggleSFX;
    }

    public void PlayMergeSound(AnimalType animalType, Vector2 position)
    {
        mergeSource.pitch = Random.Range(0.8f, 1.2f);
        mergeSource.Play();
    }

    private void ToggleSFX(bool isOn)
    {
        mergeSource.mute = !isOn;
    }

}
