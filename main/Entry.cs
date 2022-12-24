using HarmonyLib;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace main
{
    public class Entry
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Unity!");
            Console.WriteLine(string.Join(", ", args));
        }

        public string SayMyName()
        {
            return "xynta";
        }

        public void Start()
        {
            // simple way to make sure we are under .net 4.8 / C# 7.3 --- IEnumerable without generic type
            double[] arr = new double[] { 0.1, 0.2 };
            ((arr as object) as IEnumerable).Cast<double>().ToArray();

            // simply checking that Unity compilation is OK.
            var v = Vector3.one;
            Console.WriteLine(v);
        }
    }

#pragma warning disable IDE0060
    [HarmonyPatch(typeof(main.Entry))]
    [HarmonyPatch(nameof(Entry.SayMyName))]
    public class EntryPatch
    {
        static void Postfix(Entry __instance, ref string __result)
        {
            __result = "pihto";
        }
    }
}
