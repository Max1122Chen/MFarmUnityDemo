using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ParticleEffectType
{
    None,
    PineTreeLeafFalling,
    OakTreeLeafFalling,
    StoneFragment,
}

// TODO: maybe use particle ID in more complex cases, but for now we can just use the enum as the key to get the prefab from the object pool manager
[System.Serializable]
public class ParticleEffectDefinition
{
    public ParticleEffectType effectType;
    public GameObject particleEffectPrefab;
}

public class ParticleEffectManager : Singleton<ParticleEffectManager>
{
    // List of particle effect definitions corresponding to different ParticleEffectTypes
    public List<ParticleEffectDefinition> particleEffectDefinitions; 

    // Mapping from ParticleEffectType to the index of its prefab in the object pool list
    public Dictionary<ParticleEffectType, int> particleEffectTypeMapping; 

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize(Dictionary<ParticleEffectType, int> particleEffectTypeMapping)
    {
        this.particleEffectTypeMapping = particleEffectTypeMapping;
    }

    public IEnumerator PlayParticleEffect(ParticleEffectType effectType, Vector3 position)
    {
        int poolIndex = -1;
        if(effectType == ParticleEffectType.None)
        {
            yield break; // If the effect type is None, we simply exit the coroutine without doing anything.
        }

        if (particleEffectTypeMapping.TryGetValue(effectType, out poolIndex))
        {
            GameObject particleEffectPrefab = ObjectPoolManager.Instance.GetObjectFromPool(poolIndex);
            if (particleEffectPrefab != null)
            {
                particleEffectPrefab.transform.position = position;
                // You can also set the rotation or other properties of the particle effect prefab here if needed.

                // Release the particle effect prefab back to the pool after it has finished playing.
                //  This can be done by adding a script to the particle effect prefab that detects when the particle system has finished playing and then releases itself back to the pool.
                // For simplicity, we just set a hardcoded delay here to release the particle effect prefab back to the pool after 2 seconds, but in a real implementation, you would want to detect when the particle system has actually finished playing.
                
                yield return new WaitForSeconds(2f);
                ObjectPoolManager.Instance.ReleaseObjectToPool(poolIndex, particleEffectPrefab);
            }
        }
        else
        {
            Debug.LogError($"Particle effect type {effectType} not found in the mapping. Cannot play particle effect.");
        }
    }
}
