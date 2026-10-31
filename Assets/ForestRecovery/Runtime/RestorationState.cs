namespace ForestRecovery
{
    // Plain C# rules: these counts do not depend on scene objects or keyboard input.
    public sealed class RestorationState
    {
        public const int TargetTrees = 3;
        public int Seeds { get; private set; }
        public int SeedsCollected { get; private set; }

        public const int StartingHealth = 40;
        // With three plots, each successful planting adds 20 percentage points.
        public int Health => StartingHealth + (100 - StartingHealth) * TreesPlanted / TargetTrees;
        public int TreesPlanted { get; private set; }
        public bool Complete => TreesPlanted == TargetTrees;

        public bool Plant()
        {
            if (Seeds == 0 || Complete) return false;
            Seeds--;
            TreesPlanted++;
            return true;
        }

        public bool CollectSeed()
        {
            if (SeedsCollected >= TargetTrees) return false;
            SeedsCollected++;
            Seeds++;
            return true;
        }
    }
}
