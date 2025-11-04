// DarknessZone.cs
using UnityEngine;

public class DarknessZone : MonoBehaviour
{
    [Range(0f,1f)] public float darkness = 1f; // base darkness
    public float radius = 4f;

    // temporary light injections decay over time
    private float tempLight; // 0..1 (light added)
    public void InjectLight(float strength, float decayPerSecond)
    {
        tempLight = Mathf.Clamp01(Mathf.Max(tempLight, strength));
        // simple decay
        StartCoroutine(Decay(decayPerSecond));
    }

    private System.Collections.IEnumerator Decay(float perSec)
    {
        while (tempLight > 0f)
        {
            tempLight = Mathf.Max(0f, tempLight - perSec * Time.deltaTime);
            yield return null;
        }
    }

    public float SampleDarkness(Vector3 pos)
    {
        float d = Vector3.Distance(transform.position, pos);
        if (d > radius) return 0f;
        float baseVal = Mathf.Lerp(darkness, 0f, d / radius);
        float lightCut = Mathf.Clamp01(1f - tempLight); // more light → less darkness
        return baseVal * lightCut; // 1=dark, 0=bright
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0,0,0,0.2f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
