namespace CalculateAngleViaDistanceIronNest.Data {
    public class SavedAngle {
        public float velAngle { get; set; }
        public float? hozAngle { get; set; }
        public int charges { get; set; }
        public float timeToTrivel { get; set; }
        public Gun gunSelected { get; set; } = new Gun();
    }
}
