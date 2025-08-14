using System.Collections.Generic;
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }
    public List<ExperienceObject> experienceObjectsPool;
    public ExorcistLeveling exorcistLeveling;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void SpawnExperienceOrbs(Vector3 position, RealityType _realityType)
    {
        ExperienceObject experienceObject = GetExpOrbFromPool();
        experienceObject.realityType = _realityType;
        experienceObject.transform.position = position;
        experienceObject.ToggleActive(true);
    }

    private ExperienceObject GetExpOrbFromPool()
    {
        foreach (var expObject in experienceObjectsPool)
        {
            if (!expObject._active)
            {
                return expObject;
            }
        }

        // If no inactive object found, instantiate a new one
        ExperienceObject newExpObject = Instantiate(experienceObjectsPool[0]);
        experienceObjectsPool.Add(newExpObject);
        return newExpObject;
    }

    public void GiveExperienceToPlayer(int amount, RealityType realityType)
    {
        // Logic to give experience to the player
        Debug.Log($"Player received {amount} experience in {realityType}.");
        exorcistLeveling.AddExperience(amount, realityType);
        GameManager.Instance.gameUI.UpdateBars();
    }
}
