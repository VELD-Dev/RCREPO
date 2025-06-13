namespace RCRepo.Monobehaviours;

internal class CrateBehaviour : MonoBehaviour
{
    private void Start()
    {
        Plugin.logger.LogInfo($"Crate spawned at {transform.position}");
    }

    public virtual void OnCrateDestroy()
    {
        // Spawn bolts !!!!!!
    }
}
