namespace SadnessMonday.BetterPhysics.Layers {
    public enum InteractionType : short {
        Default, // the default, unmodified reaction
        Feather, // A feather interaction means the actor will not affect the receiver at all.
        Kinematic // A kinematic interaction means the receiver will not affect the actor at all.
    }
}