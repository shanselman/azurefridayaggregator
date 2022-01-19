using AFAF;

using FileStream fs1 = File.Open("dump.xml",FileMode.Create, FileAccess.Write);
await DocsToDump.DumpDoc(fs1, Format.Rss);

//using FileStream fs2 = File.Open("dump.json",FileMode.Create, FileAccess.Write);
//await DocsToDump.DumpDoc(fs2, Format.Json);