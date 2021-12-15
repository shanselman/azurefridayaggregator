using AFAF;

using FileStream fs = File.OpenWrite("dump.json");
await DocsToDump.DumpJsonFromDoc(fs);