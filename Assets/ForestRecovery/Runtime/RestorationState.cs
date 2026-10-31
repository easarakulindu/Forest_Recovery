namespace ForestRecovery
{
    // Plain C# rules: these counts do not depend on scene objects or keyboard input.
    public sealed class RestorationState
    {
        public const int TargetTrees = 3;
        public int Seeds { get; private set; }
        public int SeedsCollected { get; private set; }

        public bool CollectSeed()
        {
            if (SeedsCollected >= TargetTrees) return false;
            SeedsCollected++;
            Seeds++;
            return true;
        }
    }
}
