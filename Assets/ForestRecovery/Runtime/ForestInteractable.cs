using UnityEngine;

namespace ForestRecovery
{
    public enum ForestAction { Seed, Plant }

    public sealed class ForestInteractable : MonoBehaviour
    {
        public ForestAction action;
        public GameObject availableVisual;
        public GameObject restoredVisual;
        public bool Used { get; private set; }
        public string Label => action == ForestAction.Seed ? "COLLECT SEED" : "PLANT TREE (1 seed)";

        public bool Apply(RestorationState state)
        {
            if (Used || state == null) return false;
            bool success;
            if (action == ForestAction.Seed) success = state.CollectSeed();
            else success = state.Plant();
            if (!success) return false;
            Used = true;
            if (availableVisual != null) availableVisual.SetActive(false);
            if (restoredVisual != null) restoredVisual.SetActive(true);
            return true;
        }
    }
}
