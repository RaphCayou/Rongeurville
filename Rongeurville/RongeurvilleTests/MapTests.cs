using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rongeurville;

namespace RongeurvilleTests
{
    [TestClass]
    public class MapTests
    {
        private const string TEST_MAP = "###########################################\n" +
                                        "#   F    F               C            #   #\n" +
                                        "#                                     # R #\n" +
                                        "#     ##################       ########   #\n" +
                                        "#     #          R     #          F   #   #\n" +
                                        "#     #  F   #         #     ###      #   #\n" +
                                        "      #      #####     #     #    #       #\n" +
                                        "#     #######        ###   ###    #       #\n" +
                                        "# R                               #   ##  #\n" +
                                        "#      ####         C         #   #\n" +
                                        "###########################################";
        [TestMethod]
        public void TestExits()
        {
            Map map = Map.ParseMap(TEST_MAP);

            Assert.AreEqual(2, map.Exits.Count);
            Assert.AreEqual(new Coordinates {X=0, Y=6}, map.Exits[0].Position);
            Assert.AreEqual(new Coordinates {X=42, Y=9}, map.Exits[1].Position);
        }
    }
}
