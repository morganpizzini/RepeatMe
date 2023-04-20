namespace RepeatMe.Models
{
    public class MyProcess
    {
        public string ProcessName { get; set; }
        public int ProcessId { get; set; }
        public string ProcessDisplay => $"{ProcessName} ({ProcessId})";
        public MyProcess(string name, int id)
        {
            this.ProcessName= name;
            this.ProcessId= id;
        }
    }
}
