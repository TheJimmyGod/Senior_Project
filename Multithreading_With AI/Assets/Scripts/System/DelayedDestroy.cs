using UnityEngine;
using System.Collections;

public class DelayedDestroy : MonoBehaviour
{
    public float delay = 1.0f;
    public bool shouldRecycle = false;
    private void OnEnable()
    {
        StartCoroutine("DestroyAfterDelay");
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        if(shouldRecycle)
        {
            ServiceLocator.Get<ObjectPoolManager>().RecycleObject(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}