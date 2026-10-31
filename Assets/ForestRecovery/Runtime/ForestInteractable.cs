using UnityEngine;

namespace ForestRecovery
{
    public enum ForestAction { Seed }

    public sealed class ForestInteractable : MonoBehaviour
    {
        public ForestAction action;
        public GameObject availableVisual;
        public bool Used { get; private set; }
        public string Label => "COLLECT SEED";

        public bool Apply(RestorationState state)
        {
            if (Used || state == null) return false;
            if (!state.CollectSeed()) return false;
            Used = true;
            if (availableVisual != null) availableVisual.SetActive(false);
            return true;
        }
    }
}
