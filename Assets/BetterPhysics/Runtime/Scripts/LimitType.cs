namespace SadnessMonday.BetterPhysics {
    public enum LimitType {
        /// <summary>
        /// No limit at all.
        /// </summary>
        None = 0,
        /// <summary>
        /// A soft limit only applies to forces added through AddForce. It does not apply a limit to
        /// forces applied from external sources such as physics interactions.
        /// </summary>
        Soft,
        /// <summary>
        /// A hard limit applies to all forces and all sources of velocity. The object will be slowed
        /// to under the specified limits no matter what causes it to go over those limits.
        /// </summary>
        Hard
    }
}