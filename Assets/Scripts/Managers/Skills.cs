using System.Collections.Generic;
using UnityEngine;

public class Skills : MonoBehaviour
{
    public static Skills instance;
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

    void Awake()
    {
         if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MagicSweepSkill()
    {
        if (isOnCooldown || !PlayerDataManager.instance.UseMagicSweep() || GameManager.instance.GetGameState() != GameState.Game)
            return;
        GameOverManager.instance.SetCanLoose(false);
        AudioManager.instance.PlayMagicSound();
        GameplayController.instance.RemoveAnimalsUpTo(magicSweepUpTo, true);
        GameOverManager.instance.SetCanLoose(true);
        StartCoroutine(CooldownCoroutine());
    }

    public void UpgradeAnimalsSkill()
    {
        if (isOnCooldown || !PlayerDataManager.instance.UseUpgrade() || GameManager.instance.GetGameState() != GameState.Game)
            return;
        AudioManager.instance.PlayUpgradeSound();
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
        GameplayController.instance.SwapSpawnableAnimals(animalsToSpawn);
        StartCoroutine(RestoreAnimalsCoroutine(onProgress));
    }

    private System.Collections.IEnumerator RestoreAnimalsCoroutine(System.Action<float> onProgress)
    {
        float elapsed = animalsUpgradeDuration;
        while (elapsed > 0)
        {
            if(GameManager.instance.GetGameState() == GameState.Game)
                elapsed -= Time.deltaTime;
            onProgress?.Invoke(elapsed / animalsUpgradeDuration);
            yield return null;
        }

        onProgress?.Invoke(0f); // Ensure 0% at the end
        GameplayController.instance.RestoreSpawnableAnimals();
    }

    public void BombSkill()
    {
        if (isOnCooldown || !PlayerDataManager.instance.UseBomb() || GameManager.instance.GetGameState() != GameState.Game)
            return;
        AudioManager.instance.PlayFuseSound();
        GameplayController.instance.SetNextAnimal(AnimalType.Bomb);
        StartCoroutine(CooldownCoroutine());
    }

    private System.Collections.IEnumerator CooldownCoroutine()
    {
        isOnCooldown = true;
        magicSweepCooldownUI.Show(true);
        upgradeAnimalsCooldownUI.Show(true);
        bombCooldownUI.Show(true);

        float elapsed = cooldownDuration;
        while (elapsed > 0 )
        {
            if(GameManager.instance.GetGameState() == GameState.Game)
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

    public void Reset()
    {
        StopAllCoroutines();
        isOnCooldown = false;

        magicSweepCooldownUI.Show(false);
        upgradeAnimalsCooldownUI.Show(false);
        bombCooldownUI.Show(false);
        progressUI.Show(false);

        magicSweepCooldownUI.SetProgress(0f);
        upgradeAnimalsCooldownUI.SetProgress(0f);
        bombCooldownUI.SetProgress(0f);
        progressUI.SetProgress(0f);
    }
}