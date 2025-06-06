using System.Collections.Generic;
using UnityEngine;

public class Skills : MonoBehaviour
{
    [SerializeField] private UpgradeProgressUI progressUI;
    [SerializeField] private UpgradeProgressUI magicSweepCooldownUI;
    [SerializeField] private UpgradeProgressUI upgradeAnimalsCooldownUI;
    [SerializeField] private UpgradeProgressUI bombCooldownUI;

    [Header(" Settings ")]
    [SerializeField] private AnimalType magicSweepUpTo;
    [SerializeField] private float animalsUpgradeDuration;
    [SerializeField] private List<AnimalType> animalsToSpawn;

    [SerializeField] private float cooldownDuration = 5f;
    private bool isOnCooldown = false;

    public void MagicSweepSkill()
    {
        if (isOnCooldown || !PlayerDataManager.instance.UseMagicSweep())
            return;
        AnimalManager.instance.RemoveAnimalsUpTo(magicSweepUpTo, true);
        StartCoroutine(CooldownCoroutine());
    }

    public void UpgradeAnimalsSkill()
    {
        if (isOnCooldown || !PlayerDataManager.instance.UseUpgrade())
            return;
        progressUI.Show(true);
        StartCoroutine(CooldownCoroutine());
        UpgradeAnimalsSkill(progress =>
        {
            progressUI.SetProgress(progress);
            if (progress <= 0)
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
        float elapsed = animalsUpgradeDuration;
        while (elapsed > 0)
        {
            elapsed -= Time.deltaTime;
            onProgress?.Invoke(elapsed / animalsUpgradeDuration);
            yield return null;
        }

        onProgress?.Invoke(0f); // Ensure 0% at the end
        AnimalManager.instance.RestoreSpawnableAnimals();
    }

    public void BombSkill()
    {
        if (isOnCooldown || !PlayerDataManager.instance.UseBomb())
            return;
        AnimalManager.instance.SetNextAnimal(AnimalType.Bomb);
        StartCoroutine(CooldownCoroutine());
    }

    private System.Collections.IEnumerator CooldownCoroutine()
    {
        isOnCooldown = true;
        magicSweepCooldownUI.Show(true);
        upgradeAnimalsCooldownUI.Show(true);
        bombCooldownUI.Show(true);

        float elapsed = cooldownDuration;
        while (elapsed > 0)
        {
            elapsed -= Time.deltaTime;
            float progress = elapsed / cooldownDuration;
            magicSweepCooldownUI.SetProgress(progress);
            upgradeAnimalsCooldownUI.SetProgress(progress);
            bombCooldownUI.SetProgress(progress);
            yield return null;
        }

        magicSweepCooldownUI.SetProgress(0f);
        upgradeAnimalsCooldownUI.SetProgress(0f);
        bombCooldownUI.SetProgress(0f);

        magicSweepCooldownUI.Show(false);
        upgradeAnimalsCooldownUI.Show(false);
        bombCooldownUI.Show(false);

        isOnCooldown = false;
    }
}