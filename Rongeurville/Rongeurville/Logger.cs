using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rongeurville
{
    public class Logger
    {
        class Moves
        {
            public int NbAcceptedMoves;
            public int NbTotalMoves;
        }

        private string filename;
        private Dictionary<int, Moves> actorsMoves;
        private Func<int, ProcessType> getType;

        public Logger(Func<int, ProcessType> getType, string filename = "Log.txt")
        {
            this.getType = getType;
            this.filename = filename;
            actorsMoves = new Dictionary<int, Moves>();

            // Clean the log file
            if (File.Exists(filename))
                File.Delete(filename);
        }

        private void Log(string text, bool appendDate = true)
        {
            File.AppendAllText(filename, $"{(appendDate? $"[{DateTime.Now:HH:mm:ss}] " : "")}{text}{Environment.NewLine}");
        }
        public void LogExecutionTime(int ms, ProcessType winner)
        {
            Log($"The winner(s) are {winner.ToString().ToLower()}(s)");
            Log("====================================================");
            Log("=====================Statistics=====================");
            Log("====================================================");

            foreach (var move in actorsMoves.OrderBy(m => m.Key))
            {
                Log($"The {getType(move.Key).ToString().ToLower()} #{move.Key} request {move.Value.NbAcceptedMoves + move.Value.NbTotalMoves} moves");
                Log($"The {getType(move.Key).ToString().ToLower()} #{move.Key} has {Math.Round(move.Value.NbAcceptedMoves / (double)move.Value.NbTotalMoves * 100)}% accepted moves");
            }

            Log($"Total execution time : {ms} ms");
        }
        public void LogMove(int rank, bool accepted, Coordinates from, Coordinates to)
        {
            Log(accepted
                ? $"[Move Accepted] The {getType(rank)} #{rank} moves from {from} to {to}"
                : $"[Move Rejected] The {getType(rank)} #{rank} tried to move from {from} to {to}");

            if (!actorsMoves.ContainsKey(rank))
            {
                actorsMoves.Add(rank, new Moves { NbAcceptedMoves = accepted ? 1 : 0, NbTotalMoves = 1});
            }
            else
            {
                actorsMoves[rank].NbTotalMoves++;
                if (accepted)
                    actorsMoves[rank].NbAcceptedMoves++;
            }
        }
        public void LogCaptureRat(int ratRank, int catRank)
        {
            Log($"The cat #{catRank} capture the rat #{ratRank}");
        }
        public void LogExitRat(int ratRank, Coordinates coord)
        {
            Log($"The rat #{ratRank} exit on {coord}");
        }
        public void LogCheeseConsumption(int ratRank, Coordinates coord)
        {
            Log($"The rat #{ratRank} consume a cheese on {coord}");
        }
        public void LogMeow(int catRank, Coordinates coord)
        {
            Log($"The cat #{catRank} meow on {coord}");
        }
        public void LogMap(string map)
        {
            Log(map, false);
        }
    }
}
