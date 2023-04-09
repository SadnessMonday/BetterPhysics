namespace SadnessMonday.BetterPhysics.Layers {
    public enum InteractionType : short {
        Default, // the default, unmodified reaction
        Feather, // A feather interaction means the actor will not affect the receiver at all.
        Kinematic // A kinematic interaction means the receiver will not affect the actor at all.
    }

    public static class InteractionTypeExtensions {
        public static InteractionType Inverse(this InteractionType type) {
            switch (type) {
                case InteractionType.Feather:
                    return InteractionType.Kinematic;
                case InteractionType.Kinematic:
                    return InteractionType.Feather;
                default:
                    return type;
            }
        }
    }
}