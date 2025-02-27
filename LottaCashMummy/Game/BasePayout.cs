using System.Runtime.CompilerServices;
using LottaCashMummy.Buffer;
using LottaCashMummy.Common;
using LottaCashMummy.Table;

namespace LottaCashMummy.Game;

public class BasePayout
{
    private readonly IPayTable payTable;
    private const int LINE_LENGTH = 5;  // 릴 개수
    private const int CENTER_ROW = 1;   // 중앙 행 (0-based index)
    private readonly byte[] centerIndices; // 중앙 인덱스 배열 추가

    public BasePayout(IPayTable payTable)
    {
        this.payTable = payTable;
        this.centerIndices = new byte[LINE_LENGTH];
        for (int i = 0; i < LINE_LENGTH; i++)
        {
            centerIndices[i] = (byte)(CENTER_ROW * SlotConst.BASE_COLS + i);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CalculatePayout(BaseStorage bs)
    {
        // 중앙 라인 심볼 저장
        Span<byte> symbols = stackalloc byte[LINE_LENGTH];
        for (int i = 0; i < LINE_LENGTH; i++)
        {
            symbols[i] = bs.GetSymbol(centerIndices[i]);
        }

        byte symbol = symbols[0];

        // Wild로 시작하는 경우 Wild 조합 체크
        if (symbol == SlotConst.WILD_SYMBOL)
        {
            var wildResult = CheckWildCombination(symbols);
            if (wildResult.ShouldPay)
            {
                CalculatePayoutForSymbol(SlotConst.WILD_SYMBOL, wildResult.Count, bs);
                return;
            }

            var normalWithWildResult = CheckNormalCombinationWithWildSubstitution(symbols, wildResult.Count);
            if (normalWithWildResult.ShouldPay)
            {
                CalculatePayoutForSymbol(normalWithWildResult.Symbol, normalWithWildResult.Count, bs);
                return;
            }
        }

        var normalResult = CheckNormalCombination(symbols, symbol);
        if (normalResult.ShouldPay)
        {
            CalculatePayoutForSymbol(symbol, normalResult.Count, bs);
            return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (bool ShouldPay, int Count) CheckNormalCombination(ReadOnlySpan<byte> symbols, byte targetSymbol)
    {
        int consecutiveCount = 1;

        // 첫 번째 심볼이 Wild인 경우는 이미 Wild 조합으로 처리됨
        for (int i = 1; i < LINE_LENGTH; i++)
        {
            byte currentSymbol = symbols[i];

            // 현재 심볼이 타겟 심볼이거나 Wild인 경우
            if (currentSymbol == targetSymbol || currentSymbol == SlotConst.WILD_SYMBOL)
            {
                consecutiveCount++;
            }
            // 연속이 끊어지면 중단
            else
                break;
        }

        return (consecutiveCount >= 3, consecutiveCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (bool ShouldPay, int Count) CheckWildCombination(ReadOnlySpan<byte> symbols)
    {
        // 연속된 Wild 개수 카운트
        int consecutiveWildCount = 0;

        // 첫 번째부터 연속된 Wild 체크
        for (int i = 0; i < LINE_LENGTH; i++)
        {
            if (symbols[i] == SlotConst.WILD_SYMBOL)
            {
                consecutiveWildCount++;
            }
            else
                break;
        }

        // Wild 5개면 무조건 Wild 페이아웃
        if (consecutiveWildCount == 5)
            return (true, 5);

        // Wild 4개이고 다음이 MM/GEM이면 Wild 페이아웃
        if (consecutiveWildCount == 4 && (symbols[4] == SlotConst.MUMMY_SYMBOL || symbols[4] == SlotConst.GEM_SYMBOL))
            return (true, 4);

        // Wild 3개이고 다음이 MM/GEM이면 Wild 페이아웃
        if (consecutiveWildCount == 3 && (symbols[3] == SlotConst.MUMMY_SYMBOL || symbols[3] == SlotConst.GEM_SYMBOL))
            return (true, 3);

        return (false, consecutiveWildCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (bool ShouldPay, byte Symbol, int Count) CheckNormalCombinationWithWildSubstitution(ReadOnlySpan<byte> symbols, int wildCount)
    {
        byte nextSymbol = symbols[wildCount];

        // 다음 심볼이 일반 심볼이 아니면 페이 없음
        if (nextSymbol == SlotConst.WILD_SYMBOL ||
            nextSymbol == SlotConst.MUMMY_SYMBOL ||
            nextSymbol == SlotConst.GEM_SYMBOL)
        {
            return (false, 0, 0);
        }

        int consecutiveCount = wildCount + 1;

        // 남은 심볼들에 대해 일반 심볼 조합 체크
        for (int i = consecutiveCount; i < LINE_LENGTH; i++)
        {
            byte currentSymbol = symbols[i];

            if (currentSymbol == nextSymbol || currentSymbol == SlotConst.WILD_SYMBOL)
            {
                consecutiveCount++;
            }
            else
                break;
        }

        // 최소 3개 이상의 연속된 심볼이 있을 때만 페이
        if (consecutiveCount >= 3)
        {
            return (true, nextSymbol, consecutiveCount);
        }

        return (false, 0, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CalculatePayoutForSymbol(byte targetSymbol, int matchCount, BaseStorage bs)
    {
        var amount = payTable.GetPayout(targetSymbol, matchCount);
        if (amount > 0)
        {
            bs.AddWinAmount(targetSymbol, matchCount, amount);
        }
    }
}
