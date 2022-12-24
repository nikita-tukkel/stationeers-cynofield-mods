using HarmonyLib;
using main;
using System;
using Xunit;

namespace test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var entry = new Entry();
            Assert.Equal("xynta", entry.SayMyName());
            //Console.WriteLine(entry.SayMyName());

            var harmony = new Harmony("unique ID of my own choice");
            harmony.PatchAll(typeof(Entry).Assembly);
            Assert.Equal("pihto", entry.SayMyName());
            //Console.WriteLine(entry.SayMyName());
            harmony.UnpatchAll();
            Assert.Equal("xynta", entry.SayMyName());
            //Console.WriteLine(entry.SayMyName());
        }
    }
}