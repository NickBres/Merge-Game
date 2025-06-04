using System.Collections.Generic;
using UnityEngine;

public class Skills : MonoBehaviour
{
    [SerializeField] private UpgradeProgressUI progressUI;

    [Header(" Settings ")]
    [SerializeField] private AnimalType magicSweepUpTo;
    [SerializeField] private float animalsUpgradeDuration;
    [SerializeField] private List<AnimalType> animalsToSpawn;
    public void MagicSweepSkill()
    {
        AnimalManager.instance.RemoveAnimalsUpTo(magicSweepUpTo, true);
    }

    public void UpgradeAnimalsSkill()
    {
        progressUI.Show(true);
        UpgradeAnimalsSkill(progress =>
        {
            progressUI.SetProgress(progress);
            if (progress >= 1f)
                progressUI.Show(false);
        });
    }

    private void UpgradeAnimalsSkill(System.Action<float> onProgress)
    {
        AnimalManager.instance.SwapSpawnableAnimals(animalsToSpawn);
        StartCoroutine(RestoreAnimalsCoroutine(onProgress));
    }

    private System.Collections.IEnumerator RestoreAnimalsCoroutine(System.Action<float> onProgress)
    {
        float elapsed = 0f;
        while (elapsed < animalsUpgradeDuration)
        {
            elapsed += Time.deltaTime;
            onProgress?.Invoke(elapsed / animalsUpgradeDuration);
            yield return null;
        }

        onProgress?.Invoke(1f); // Ensure 100% at the end
        AnimalManager.instance.RestoreSpawnableAnimals();
    }

    public void BombSkill()
    {
        AnimalManager.instance.SetNextAnimal(AnimalType.Bomb);
    }
}
