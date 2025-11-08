using UnityEngine;
public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public void PlayParticleEffect(GameObject particlePrefab, Vector3 position)
    {
        if (particlePrefab == null)
        {
            Debug.LogWarning("Tried to play a particle effect with a null prefab.");
            return;
        }

        GameObject particleEffectInstance = Instantiate(particlePrefab, position, Quaternion.identity);

        ParticleSystem ps = particleEffectInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            Destroy(particleEffectInstance, ps.main.duration);
        }
        else
        {
            Debug.LogWarning("The prefab does not have a ParticleSystem component. It will be destroyed after 5 seconds.");
            Destroy(particleEffectInstance, 5f);
        }
    }
}