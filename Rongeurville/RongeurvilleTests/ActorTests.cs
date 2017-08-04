using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rongeurville;

namespace RongeurvilleTests
{
    [TestClass]
    public class ActorTests
    {
        [TestMethod]
        public void TestPathFinding()
        {
            const string TEST_MAP =
            "###########################################\n" +
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
            Map mapTest = Map.ParseMap(TEST_MAP);
            Rat ratTest = new Rat();
            Rat ratTest2 = new Rat();
            Rat ratTest3 = new Rat();
            Cat catTest = new Cat();
            Cat catTest2 = new Cat();
            ratTest.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(1));
            Tuple<Coordinates, int> ratMove = ratTest.GetDirection();
            ratTest2.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(2));
            ratTest3.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(3));
            catTest2.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(4));
            catTest.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(5));
            Tuple<Coordinates, int> catMove = catTest.GetDirection();
            Tuple<Coordinates, int> catMove2 = catTest2.GetDirection();
            Tuple<Coordinates, int> ratMove2 = ratTest2.GetDirection();
            Tuple<Coordinates, int> ratMove3 = ratTest3.GetDirection();

            Assert.AreEqual(8, catMove.Item2);
            Assert.AreEqual(20, catMove.Item1.X);
            Assert.AreEqual(8, catMove.Item1.Y);

            Assert.AreEqual(19, catMove2.Item2);
            Assert.AreEqual(25, catMove2.Item1.X);
            Assert.AreEqual(2, catMove2.Item1.Y);

            Assert.AreEqual(8, ratMove.Item2);
            Assert.AreEqual(40, ratMove.Item1.X);
            Assert.AreEqual(3, ratMove.Item1.Y);

            Assert.AreEqual(8, ratMove2.Item2);
            Assert.AreEqual(16, ratMove2.Item1.X);
            Assert.AreEqual(5, ratMove2.Item1.Y);
            //ratMove2 = ratTest2.GetDirection(true);
            //Assert.AreEqual(7, ratMove2.Item2);
            //Assert.AreEqual(15, ratMove2.Item1.X);
            //Assert.AreEqual(4, ratMove2.Item1.Y);
            //Assert.AreEqual(6, ratMove2.Item2);
            //Assert.AreEqual(14, ratMove2.Item1.X);
            //Assert.AreEqual(5, ratMove2.Item1.Y);
            //Assert.AreEqual(5, ratMove2.Item2);
            //Assert.AreEqual(13, ratMove2.Item1.X);
            //Assert.AreEqual(4, ratMove2.Item1.Y);

            Assert.AreEqual(7, ratMove3.Item2);
            Assert.AreEqual(2, ratMove3.Item1.X);
            Assert.AreEqual(7, ratMove3.Item1.Y);
        }

        [TestMethod]
        public void PreciseTestPathFinding()
        {
            const string TEST_MAP2 = "###########################################\n" +
                                     "#   F    F                            #   #\n" +
                                     "#  R C                                #   #\n" +
                                     "#   R ##################       ########   #\n" +
                                     "#     #                #          F   #   #\n" +
                                     "#     #  F   #         #     ###      #   #\n" +
                                     "      #      #####     #     #    #       #\n" +
                                     "#     #######        ###   ###    #       #\n" +
                                     "#                                 #   ##  #\n" +
                                     "#      ####                   #   #\n" +
                                     "###########################################";
            Map mapTest = Map.ParseMap(TEST_MAP2);
            Rat ratTest = new Rat();
            Rat ratTest2 = new Rat();
            Cat catTest = new Cat();
            ratTest.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(1));
            ratTest2.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(2));
            catTest.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(3));
            Tuple<Coordinates, int> catMove = catTest.GetDirection();
            Tuple<Coordinates, int> ratMove = ratTest.GetDirection();
            Tuple<Coordinates, int> ratMove2 = ratTest2.GetDirection();

            Assert.AreEqual(1, ratMove.Item2);
            Assert.AreEqual(4, ratMove.Item1.X);
            Assert.AreEqual(1, ratMove.Item1.Y);

            Assert.AreEqual(2, ratMove2.Item2);
            Assert.AreEqual(4, ratMove2.Item1.X);
            Assert.AreEqual(2, ratMove2.Item1.Y);

            Assert.AreEqual(2, catMove.Item2);
            Assert.AreEqual(5, catMove.Item1.X);
            Assert.AreEqual(3, catMove.Item1.Y);
        }

        [TestMethod]
        public void ManualTestPathFinding()
        {
            const string TEST_MAP3 = "###########################################\n" +
                                     "#   F    F                            #   #\n" +
                                     "#  R                                  #   #\n" +
                                     "#     ##################       ########   #\n" +
                                     "#     #                #          F   #   #\n" +
                                     "#     #  F   #         #     ###      #   #\n" +
                                     "    C #      #####     #     #    #       #\n" +
                                     "#     #######        ###   ###    #       #\n" +
                                     "#                                 #   ##  #\n" +
                                     "#      ####                   #   #\n" +
                                     "###########################################";
            Map mapTest = Map.ParseMap(TEST_MAP3);
            Rat ratTest = new Rat();
            Cat catTest = new Cat();
            ratTest.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(1));
            catTest.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(2));
            Tuple<Coordinates, int> catMove = catTest.GetDirection();
            Tuple<Coordinates, int> ratMove = ratTest.GetDirection();

            Assert.AreEqual(1, ratMove.Item2);
            Assert.AreEqual(4, ratMove.Item1.X);
            Assert.AreEqual(1, ratMove.Item1.Y);

            //Assert.AreEqual(3, catMove.Item2);
            //Assert.AreEqual(4, catMove.Item1.X);
            //Assert.AreEqual(5, catMove.Item1.Y);
        }

        [TestMethod]
        public void NeighBorsTest()
        {
            const string TEST_MAP4 = "###########################################\n" +
                                     "#R  F    F          R                 ##R##\n" +
                                     "#                                     #   #\n" +
                                     "#     ##################       ########   #\n" +
                                     "#     #      R         #          F   #   #\n" +
                                     "#     #  F   #         #     ###      #   #\n" +
                                     "RR  C #      #####     #     #    #      R#\n" +
                                     "#     #######        ###   ###    #       #\n" +
                                     "#                                 #   ##  #\n" +
                                     "#R     ####                  R# FR\n" +
                                     "###########################################";
            Rat ratTest = new Rat();
            Rat ratTest2 = new Rat();
            Rat ratTest3 = new Rat();
            Rat ratTest35 = new Rat();
            Rat ratTest4 = new Rat();
            Rat ratTest5 = new Rat();
            Rat ratTest6 = new Rat();
            Rat ratTest7 = new Rat();
            Rat ratTest8 = new Rat();
            Rat ratTest9 = new Rat();
            Map mapTest = Map.ParseMap(TEST_MAP4);
            ratTest.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(1));
            ratTest2.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(2));
            ratTest3.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(3));
            ratTest35.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(4));
            ratTest4.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(5));
            ratTest5.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(6));
            ratTest6.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(7));
            ratTest7.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(8));
            ratTest8.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(9));
            ratTest9.SetMapAndCurrentTile(mapTest, mapTest.GetCurrentTileByRank(10));
            List<Tile> ratMoves = ratTest.GetNeighbors(ratTest.CurrentTile);
            Assert.AreEqual(3, ratMoves.Count);

            List<Tile> ratMoves2 = ratTest2.GetNeighbors(ratTest2.CurrentTile);
            Assert.AreEqual(5, ratMoves2.Count);

            List<Tile> ratMoves3 = ratTest3.GetNeighbors(ratTest3.CurrentTile);
            Assert.AreEqual(3, ratMoves3.Count);

            List<Tile> ratMoves35 = ratTest35.GetNeighbors(ratTest35.CurrentTile);
            Assert.AreEqual(4, ratMoves35.Count);

            List<Tile> ratMoves4 = ratTest4.GetNeighbors(ratTest4.CurrentTile);
            Assert.AreEqual(2, ratMoves4.Count);

            List<Tile> ratMoves5 = ratTest5.GetNeighbors(ratTest5.CurrentTile);
            Assert.AreEqual(5, ratMoves5.Count);

            List<Tile> ratMoves6 = ratTest6.GetNeighbors(ratTest6.CurrentTile);
            Assert.AreEqual(5, ratMoves6.Count);

            List<Tile> ratMoves7 = ratTest7.GetNeighbors(ratTest7.CurrentTile);
            Assert.AreEqual(3, ratMoves7.Count);

            List<Tile> ratMoves8 = ratTest8.GetNeighbors(ratTest8.CurrentTile);
            Assert.AreEqual(4, ratMoves8.Count);

            List<Tile> ratMoves9 = ratTest9.GetNeighbors(ratTest9.CurrentTile);
            Assert.AreEqual(4, ratMoves9.Count);
        }
    }
}
