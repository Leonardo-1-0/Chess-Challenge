using System;
using ChessChallenge.API;


//TODO: Depth search
//TODO: Time-dependent depth throttling
public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        int depth = 3;
        Move[] moves = board.GetLegalMoves();
        Move moveToPlay = moves[0];
        double bestMoveScore = double.NegativeInfinity;

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            double score = AlphaBetaMin(
                board,double.NegativeInfinity, double.PositiveInfinity, depth);
            board.UndoMove(move);
            
            if (score > bestMoveScore)
            {
                bestMoveScore = score;
                moveToPlay = move;
            }
        }

        return moveToPlay;
    }


//    TODO: Trade down bonus for winning side
//    TODO: Increase value of center pawns, decrease side pawns
//    TODO: No pawn penalty (harder to win endgames)
//    TODO: Penalty for rook pair
//    TODO: Penalty for knight pair
//    TODO: Discourage bad trades (2 pieces for rook / 3 pawns for piece)
//    TODO: Introduce game stage (i.e. opening/middlegame/endgame) as a function of material on the board
    // EVALUATE
    public double Evaluate(Board board)
    {
        int whoToPlay = board.IsWhiteToMove ? 1 : -1;
        double evaluation = 0;
        
        if (board.IsInCheckmate())
        {
            evaluation = board.IsWhiteToMove ? double.NegativeInfinity : double.PositiveInfinity;
            return evaluation;
        }

//        MATERIAL
        PieceList whiteKing = board.GetPieceList(PieceType.King, true);
        PieceList whiteQueen = board.GetPieceList(PieceType.Queen, true);
        PieceList whiteRooks = board.GetPieceList(PieceType.Rook, true);
        PieceList whiteBishops = board.GetPieceList(PieceType.Bishop, true);
        PieceList whiteKnights = board.GetPieceList(PieceType.Knight, true);
        PieceList whitePawns = board.GetPieceList(PieceType.Pawn, true);

        PieceList blackKing = board.GetPieceList(PieceType.King, false);
        PieceList blackQueen = board.GetPieceList(PieceType.Queen, false);
        PieceList blackRooks = board.GetPieceList(PieceType.Rook, false);
        PieceList blackBishops = board.GetPieceList(PieceType.Bishop, false);
        PieceList blackKnights = board.GetPieceList(PieceType.Knight, false);
        PieceList blackPawns = board.GetPieceList(PieceType.Pawn, false);


        evaluation += 200 * (whiteKing.Count - blackKing.Count);
        evaluation += 9 * (whiteQueen.Count - blackQueen.Count);
        evaluation += 5 * (whiteRooks.Count - blackRooks.Count);
        evaluation += 3 * (whiteBishops.Count - blackBishops.Count);
        evaluation += 3 * (whiteKnights.Count - blackKnights.Count);
        evaluation += 1 * (whitePawns.Count - blackPawns.Count);

        // bishop pair bonus
        if (!(whiteBishops.Count == 2 && blackBishops.Count == 2) &&
            (whiteBishops.Count == 2 || blackBishops.Count == 2))
        {
            evaluation += whiteBishops.Count - blackBishops.Count > 0 ? 0.25 : -0.25;
        }

//        POSITIONAL

        // mobility bonus
        Move[] whiteMoves = null;
        Move[] blackMoves = null;
        if (board.IsWhiteToMove)
        {
            whiteMoves = board.GetLegalMoves();
            board.ForceSkipTurn();
            blackMoves = board.GetLegalMoves();
            board.UndoSkipTurn();
        }
        else
        {
            blackMoves = board.GetLegalMoves();
            board.ForceSkipTurn();
            whiteMoves = board.GetLegalMoves();
            board.UndoSkipTurn();
        }
        evaluation += 0.1 * (whiteMoves.Length - blackMoves.Length);

        return evaluation * whoToPlay;
    }

    
//    TODO: Optimize with iterative deepening, transposition table, quiescence search
    // SEARCH (alpha-beta tree)
    double AlphaBetaMax(Board board, double alpha, double beta, int depthleft)
    {
        if (depthleft == 0)
        {
            return Evaluate(board);
        }
        Move[] moves = board.GetLegalMoves();
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            double score = AlphaBetaMin(board, alpha, beta, depthleft - 1);
            board.UndoMove(move);
            if (score >= beta)
                return beta; // fail hard beta-cutoff
            if (score > alpha)
                alpha = score; // alpha acts like max in MiniMax
            if (beta <= alpha)
                break;
        }

        return alpha;
    }

    double AlphaBetaMin(Board board, double alpha, double beta, int depthleft)
    {
        if (depthleft == 0)
        {
            return -Evaluate(board);
        }
        Move[] moves = board.GetLegalMoves();
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            double score = AlphaBetaMax(board, alpha, beta, depthleft - 1);
            board.UndoMove(move);
            if (score <= alpha)
                return alpha; // fail hard alpha-cutoff
            if (score < beta)
                beta = score; // beta acts like min in MiniMax
            if (beta <= alpha)
                break;
        }

        return beta;
    }
}
