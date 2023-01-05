using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace cynofield.mods.utils
{
    public class TagParserTest
    {
        TagParser parser = new TagParser();

        [Fact]
        public void Test1()
        {
            Assert.Null(parser.Parse(null));
            Assert.Null(parser.Parse(""));
            Assert.Null(parser.Parse("  "));
            Assert.Null(parser.Parse("nothing"));
        }

        [Fact]
        public void Test2()
        {
            Assert.Null(parser.Parse("anything#"));
            Assert.Null(parser.Parse("anything# "));
            Assert.Null(parser.Parse("# "));
            Assert.NotNull(parser.Parse("#tag #tag1 #tag2 nottag #othertag"));
        }

        [Fact]
        public void Test3()
        {
            var result = parser.Parse("#tag #tag1 #tag2 nottag #othertag ###mixed#tag3 inner#tag4");
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
            int i = 0;
            Assert.Equal("#tag", result[i++].name);
            Assert.Equal("#tag", result[i++].name);
            Assert.Equal("#tag", result[i++].name);
            Assert.Equal("#othertag", result[i++].name);
            Assert.Equal("#mixed", result[i++].name);
            Assert.Equal("#tag", result[i++].name);
            Assert.Equal("#tag", result[i++].name);
        }

        [Fact]
        public void Test4()
        {
            var result = parser.Parse("#tag1_alpha-44#tag____once");
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            {
                var tag = result[0];
                Assert.Equal("#tag", tag.name);
                Assert.Equal(3, tag.paramsString.Length);
                Assert.Equal(2, tag.paramsInt.Length);

                Assert.Equal("1", tag.paramsString[0]);
                Assert.Equal("alpha", tag.paramsString[1]);
                Assert.Equal("44", tag.paramsString[2]);

                Assert.Equal(1, tag.paramsInt[0]);
                Assert.Equal(44, tag.paramsInt[1]);
            }
            {
                var tag = result[1];
                Assert.Equal("#tag", tag.name);
                Assert.Single(tag.paramsString);
                Assert.Null(tag.paramsInt);

                Assert.Equal("once", tag.paramsString[0]);
            }
        }

        [Fact]
        public void Test5()
        {
            var result = parser.Parse("Filtration #w");
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("#w", result[0].name);
            Assert.Null(result[0].paramsString);
            Assert.Null(result[0].paramsInt);
        }
    }
}
