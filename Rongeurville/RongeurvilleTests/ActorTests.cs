using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rongeurville;

namespace RongeurvilleTests
{
    [TestClass]
    public class ActorTests
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
        public void TestPathFinding()
        {
            Map mapTest = Map.ParseMap(TEST_MAP);
            Rat ratTest = new Rat();
            Cat catTest = new Cat();
            ratTest.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(1));
            catTest.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(5));
            Tuple<Coordinates, int> ratMove = ratTest.GetDirection();
            Tuple<Coordinates, int> catMove = catTest.GetDirection();
            Assert.AreEqual(8, catMove.Item2);
            Assert.AreEqual(19, catMove.Item1.X);
            Assert.AreEqual(9, catMove.Item1.Y);
            Assert.AreEqual(8, ratMove.Item2);
            Assert.AreEqual(41, ratMove.Item1.X);
            Assert.AreEqual(3, ratMove.Item1.Y);
        }
    }
}
