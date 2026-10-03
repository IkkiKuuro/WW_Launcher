using System;
using System.Reflection;
class Program {
    static void Main() {
        try {
            Assembly asm = Assembly.LoadFrom(@"C:\Users\irani\OneDrive\Documentos\GitHub\WW_Launcher\API\Client\WWClient.dll");
            foreach (Type t in asm.GetTypes()) {
                if (t.Name == "Client") {
                    Console.WriteLine("Type: " + t.FullName);
                }
            }
        } catch(ReflectionTypeLoadException e) { 
            foreach(var ex in e.LoaderExceptions) Console.WriteLine(ex);
        }
    }
}
