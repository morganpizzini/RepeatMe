using System.Collections.Generic;
using System.Linq;

namespace RepeatMe.Models
{
    public abstract class RenameClass
    {
        public static Dictionary<int,string> Names { get; set; } = new Dictionary<int, string>();
    }
    public class MyProcess
    {
        public string ProcessName { get; set; }
        public int ProcessId { get; set; }
        public uint WindowsProcessId { get; set; }
        public string ProcessDisplay => RenameClass.Names.ContainsKey(ProcessId) ? $"{RenameClass.Names[ProcessId]} ({ProcessId})" : $"{ProcessName} ({ProcessId})";
        public MyProcess(string name, int id, uint windowsId)
        {
            this.ProcessName= name;
            this.WindowsProcessId = windowsId;
            this.ProcessId= id;
        }
    }
}
