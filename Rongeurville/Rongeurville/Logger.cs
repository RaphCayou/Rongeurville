using System;
using System.Collections.Generic;
using System.IO;

namespace Rongeurville
{
    public class Logger
    {
        class Pair
        {
            public int NbrAccepted;
            public int NbrRejected;
        }
        private string filename;
        private Dictionary<int, Pair> moves;
        private Func<int, ProcessType> getType;

        public Logger(Func<int, ProcessType> getType, string filename = "Log.txt")
        {
            this.getType = getType;
            this.filename = filename;
            moves = new Dictionary<int, Pair>();

            // Clean the log file
            if (File.Exists(filename))
                File.Delete(filename);
        }

        private void Log(string text)
        {
            File.AppendAllText(filename, text + Environment.NewLine);
        }
        public void LogExecutionTime(int ms)
        {
            Log("====================================================");
            Log("=====================Statistics=====================");
            Log("====================================================");
            foreach (var move in moves)
            {
                Log($"The {getType(move.Key).ToString().ToLower()} #{move.Key} request {move.Value.NbrAccepted + move.Value.NbrRejected} moves");
                Log($"The {getType(move.Key).ToString().ToLower()} #{move.Key} has a proportion of {move.Value.NbrAccepted / (double)(move.Value.NbrAccepted + move.Value.NbrRejected)} accepted moves");
            }
            Log($"Total execution time : {ms} ms");
        }
        public void LogMove(int rank, bool accepted, Coordinates from, Coordinates to)
        {
            // TODO Delete this, for debug purpose only
            Log(accepted
                ? $"+ The {getType(rank)} #{rank} moves from {from} to {to}"
                : $"- The {getType(rank)} #{rank} tried to move from {from} to {to}");

            if (!moves.ContainsKey(rank))
            {
                moves.Add(rank, new Pair { NbrAccepted = accepted ? 1 : 0, NbrRejected = accepted ? 0 : 1 });
            }
            else
            {
                if (accepted)
                    moves[rank].NbrAccepted++;
                else
                    moves[rank].NbrRejected++;
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
            Log(map);
        }
    }
}
