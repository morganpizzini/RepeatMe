using System.Collections.Generic;

namespace RepeatMe.Models
{
    public class Setting
    {
        public int StandardDelayTime { get; set; }
        public int SensibilityRange { get; set; }
        public int MaxRotationNumber { get; set; }
        public string DefaultKeyPress { get; set; }

        public Dictionary<string, int[]> PressKeys { get; set; } = new Dictionary<string, int[]>();
        public double Interval { get; set; }
    }
}
