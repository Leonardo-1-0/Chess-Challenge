using ChessChallenge.API;
using System;
using ChessChallenge.Application;

namespace ChessChallenge.Example
{
    public class EvilBot : IChessBot
    {
        private enum NodeType
        {
            Exact, UpperBound, LowerBound
        }
        //{ Zobrist-key, board state, depth, eval, node-type, age }
        private readonly (ulong, Board, int, int, int)[] transpositionTable = new (ulong, Board, int, int, int)[0x400000];
        public Move Think(Board board, Timer timer)
        {
            int depth = 3;
            int maxScore = Int32.MinValue;
            Move bestMove = Move.NullMove;
            
            Span<Move> moves = stackalloc Move[256];
            board.GetLegalMovesNonAlloc(ref moves);
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int score = NegamaxAlphaBeta(board, 3, Int32.MinValue, Int32.MaxValue);
                board.UndoMove(move);
                if (score > maxScore)
                {
                    maxScore = score;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private int SimpleEval(Board board)
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

        
        private int NegamaxAlphaBeta(Board board, int depth, int alpha, int beta)
        {
            if (depth <= 0 || board.IsInCheckmate() || board.IsInStalemate() || board.IsInsufficientMaterial())
            {
                return SimpleEval(board);
            }
            
            int maxScore = Int32.MinValue;
            Span<Move> moves = stackalloc Move[256];
            board.GetLegalMovesNonAlloc(ref moves);
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int score = -NegamaxAlphaBeta(board, depth - 1, -alpha, -beta);
                board.UndoMove(move);
                maxScore = Math.Max(maxScore, score);
                alpha = Math.Max(alpha, score);
                if (alpha >= beta)
                {
                    return alpha;
                }
            }

            return maxScore;
        }
    }
}