using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [Header(" Elements ")]
    [SerializeField] private AudioSource mergeSource;
    [SerializeField] private AudioSource explosionSource;



    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
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

    public void PlayExplosionSound(Vector2 position)
    {
        // Implement explosion sound logic here if needed
        explosionSource.pitch = Random.Range(0.8f, 1.2f);
        explosionSource.Play();
    }

    private void ToggleSFX(bool isOn)
    {
        mergeSource.mute = !isOn;
    }

}
