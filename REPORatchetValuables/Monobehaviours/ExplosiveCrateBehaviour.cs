namespace RCRepo.Monobehaviours
{
    internal class ExplosiveCrateBehaviour : CrateBehaviour
    {
        private void Start()
        {
            // Initialize the explosive crate behavior
        }
        public override void OnCrateDestroy()
        {
            base.OnCrateDestroy();
            // Handle the destruction of the explosive crate
            // This could include spawning an explosion effect, damaging nearby entities
        }
    }
}
