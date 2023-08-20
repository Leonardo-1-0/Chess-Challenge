using ChessChallenge.API;
using System;
using ChessChallenge.Application;

namespace ChessChallenge.Example
{
    public class EvilBot : IChessBot
    {
        public Move Think(Board board, Timer timer)
        {
            return Search(board);
        }

        public int SimpleEval(Board board)
        {
            int whoToPlay = board.IsWhiteToMove ? 1 : -1;

            if (board.IsInCheckmate())
            {
                return 10_000 * whoToPlay;
            }
            
            int score = 1_000 *
                        (board.GetPieceList(PieceType.King, true).Count -
                         board.GetPieceList(PieceType.King, false).Count);
            score += 9 *
                     (board.GetPieceList(PieceType.Queen, true).Count -
                      board.GetPieceList(PieceType.Queen, false).Count);
            score += 5 *
                     (board.GetPieceList(PieceType.Rook, true).Count - board.GetPieceList(PieceType.Rook, false).Count);
            score += 3 *
                     (board.GetPieceList(PieceType.Bishop, true).Count -
                      board.GetPieceList(PieceType.Bishop, false).Count);
            score += 3 *
                     (board.GetPieceList(PieceType.Knight, true).Count -
                      board.GetPieceList(PieceType.Knight, false).Count);
            score += 1 *
                     (board.GetPieceList(PieceType.Pawn, true).Count - board.GetPieceList(PieceType.Pawn, false).Count);

            return score * whoToPlay;
        }

        public Move Search(Board board)
        {
            int maxScore = Int32.MinValue;
            Move bestMove = Move.NullMove;
            Move[] moves = board.GetLegalMoves();
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int score = SimpleEval(board);
                board.UndoMove(move);
                if (score > maxScore)
                {
                    bestMove = move;
                }
            }

            return bestMove;
        }
    }
}