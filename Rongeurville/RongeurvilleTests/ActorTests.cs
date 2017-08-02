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
        private const string TEST_MAP2 = "###########################################\n" +
                                        "#   F    F                            #   #\n" +
                                        "#    C                                #   #\n" +
                                        "#   R ##################       ########   #\n" +
                                        "#     #                #          F   #   #\n" +
                                        "#     #  F   #         #     ###      #   #\n" +
                                        "      #      #####     #     #    #       #\n" +
                                        "#     #######        ###   ###    #       #\n" +
                                        "#                                 #   ##  #\n" +
                                        "#      ####                   #   #\n" +
                                        "###########################################";
        private const string TEST_MAP3 = "###########################################\n" +
                                         "#   F    F                            #   #\n" +
                                         "#                                     #   #\n" +
                                         "#   R ##################       ########   #\n" +
                                         "#     #                #          F   #   #\n" +
                                         "#     #  F   #         #     ###      #   #\n" +
                                         "    C #      #####     #     #    #       #\n" +
                                         "#     #######        ###   ###    #       #\n" +
                                         "#                                 #   ##  #\n" +
                                         "#      ####                   #   #\n" +
                                         "###########################################";

        [TestMethod]
        public void TestPathFinding()
        {
            Map mapTest = Map.ParseMap(TEST_MAP);
            Rat ratTest = new Rat();
            Rat ratTest2 = new Rat();
            Rat ratTest3 = new Rat();
            Cat catTest = new Cat();
            Cat catTest2 = new Cat();
            ratTest.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(1));
            ratTest2.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(2));
            ratTest3.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(3));
            catTest2.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(4));
            catTest.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(5));
            Tuple<Coordinates, int> catMove = catTest.GetDirection();
            Tuple<Coordinates, int> catMove2 = catTest2.GetDirection();
            Tuple<Coordinates, int> ratMove = ratTest.GetDirection();
            Tuple<Coordinates, int> ratMove2 = ratTest2.GetDirection();
            Tuple<Coordinates, int> ratMove3 = ratTest3.GetDirection();

            Assert.AreEqual(8, catMove.Item2);
            Assert.AreEqual(19, catMove.Item1.X);
            Assert.AreEqual(9, catMove.Item1.Y);

            Assert.AreEqual(19, catMove2.Item2);
            Assert.AreEqual(25, catMove2.Item1.X);
            Assert.AreEqual(2, catMove2.Item1.Y);

            Assert.AreEqual(8, ratMove.Item2);
            Assert.AreEqual(41, ratMove.Item1.X);
            Assert.AreEqual(3, ratMove.Item1.Y);

            Assert.AreEqual(8, ratMove2.Item2);
            Assert.AreEqual(16, ratMove2.Item1.X);
            Assert.AreEqual(4, ratMove2.Item1.Y);

            Assert.AreEqual(7, ratMove3.Item2);
            Assert.AreEqual(3, ratMove3.Item1.X);
            Assert.AreEqual(7, ratMove3.Item1.Y);
        }

        [TestMethod]
        public void PreciseTestPathFinding()
        {
            Map mapTest = Map.ParseMap(TEST_MAP2);
            Rat ratTest = new Rat();
            Cat catTest = new Cat();
            ratTest.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(1));
            catTest.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(2));
            Tuple<Coordinates, int> catMove = catTest.GetDirection();
            Tuple<Coordinates, int> ratMove = ratTest.GetDirection();

            Assert.AreEqual(2, ratMove.Item2);
            Assert.AreEqual(4, ratMove.Item1.X);
            Assert.AreEqual(2, ratMove.Item1.Y);

            Assert.AreEqual(2, catMove.Item2);
            Assert.AreEqual(5, catMove.Item1.X);
            Assert.AreEqual(3, catMove.Item1.Y);
        }

        [TestMethod]
        public void ManualTestPathFinding()
        {
            Map mapTest = Map.ParseMap(TEST_MAP3);
            Rat ratTest = new Rat();
            Cat catTest = new Cat();
            ratTest.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(1));
            catTest.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(2));
            Tuple<Coordinates, int> catMove = catTest.GetDirection();
            Tuple<Coordinates, int> ratMove = ratTest.GetDirection();

            Assert.AreEqual(2, ratMove.Item2);
            Assert.AreEqual(4, ratMove.Item1.X);
            Assert.AreEqual(2, ratMove.Item1.Y);

            Assert.AreEqual(3, catMove.Item2);
            Assert.AreEqual(4, catMove.Item1.X);
            Assert.AreEqual(5, catMove.Item1.Y);
        }
    }
}
