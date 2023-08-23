// See https://aka.ms/new-console-template for more information
using Test;



TableMgr.Inst.LoadAllTable();
TableMgr.Get(1, 2, out TTestComposeKey v);
v = TableMgr.GetTTestComposeKey(1, 2);
Console.WriteLine("Hello, World! " + v.Name);

