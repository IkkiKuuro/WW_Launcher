using System;
using System.Reflection;
class Program {
    static void Main() {
        try {
            Assembly asm = Assembly.LoadFrom(@"C:\Users\irani\OneDrive\Documentos\GitHub\WW_Launcher\API\Client\WWClient.dll");
            Type t = asm.GetType("WWClient.Network.Client");
            if (t == null) { Console.WriteLine("Type not found."); return; }
            foreach (var p in t.GetProperties()) Console.WriteLine("Prop: " + p.Name);
            foreach (var f in t.GetFields()) Console.WriteLine("Field: " + f.Name);
        } catch(Exception e) { Console.WriteLine(e); }
    }
}
