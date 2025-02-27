// using LottaCashMummy.Common;
// using System.Runtime.CompilerServices;

// namespace LottaCashMummy.Calculator;

// public class AreaCalculator
// {
//     private static readonly Dictionary<int, int> AreaToSideLength = new()
//     {
//         { 4, 2 },   // 2x2
//         { 9, 3 },   // 3x3
//         { 16, 4 },  // 4x4
//         { 25, 5 }   // 5x5
//     };

//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static int CalculateAreaAndCenter(
//         Span<byte> area,
//         int centerIndex,
//         int blockSize)
//     {
//         if ((uint)centerIndex >= SlotConst.FEATURE_ROWS * SlotConst.FEATURE_COLS)
//             throw new ArgumentOutOfRangeException(nameof(centerIndex));

//         if (blockSize is not (4 or 9 or 16 or 25))
//             throw new ArgumentOutOfRangeException(nameof(blockSize));

//         area.Clear();

//         int currentRow = (int)((uint)centerIndex / SlotConst.FEATURE_COLS);
//         int currentCol = centerIndex - (currentRow * SlotConst.FEATURE_COLS);

//         if (!AreaToSideLength.TryGetValue(blockSize, out var sideLength))
//         {
//             throw new InvalidOperationException("Invalid area size");
//         }

//         int offset = (sideLength - 1) >> 1;

//         int startRow = Math.Max(0, Math.Min(SlotConst.FEATURE_ROWS - sideLength, currentRow - offset));
//         int startCol = Math.Max(0, Math.Min(SlotConst.FEATURE_COLS - sideLength, currentCol - offset));

//         int rowOffset = startRow * SlotConst.FEATURE_COLS;
//         for (int row = 0; row < sideLength; row++)
//         {
//             int baseIndex = rowOffset + startCol;
//             int count = sideLength;

//             var slice = area.Slice(baseIndex, count);
//             slice.Fill(1);

//             rowOffset += SlotConst.FEATURE_COLS;
//         }

//         return (startRow + (sideLength >> 1)) * SlotConst.FEATURE_COLS + (startCol + (sideLength >> 1));
//     }

//     // public static (int NewCenterIndex, bool Collected) MoveAreaToItem(
//     // Span<byte> area,
//     // int currentIndex,
//     // int itemIndex,
//     // int blockSize)
//     // {
//     //     // 현재 위치
//     //     int currentRow = (int)((uint)currentIndex / SlotConst.FEATURE_COLS);
//     //     int currentCol = currentIndex - (currentRow * SlotConst.FEATURE_COLS);

//     //     // 아이템 위치
//     //     int itemRow = (int)((uint)itemIndex / SlotConst.FEATURE_COLS);
//     //     int itemCol = itemIndex - (itemRow * SlotConst.FEATURE_COLS);

//     //     // 현재 영역에서 아이템이 포함되어 있는지 확인
//     //     if (IsItemInArea(area, itemIndex))
//     //     {
//     //         return (currentIndex, true);
//     //     }

//     //     // 이동 방향 결정 (대각선 이동 허용)
//     //     int newRow = currentRow;
//     //     int newCol = currentCol;

//     //     // 행 이동
//     //     if (currentRow < itemRow) newRow++;
//     //     else if (currentRow > itemRow) newRow--;

//     //     // 열 이동
//     //     if (currentCol < itemCol) newCol++;
//     //     else if (currentCol > itemCol) newCol--;

//     //     // 경계 검사
//     //     newRow = Math.Clamp(newRow, 0, SlotConst.FEATURE_ROWS - 1);
//     //     newCol = Math.Clamp(newCol, 0, SlotConst.FEATURE_COLS - 1);

//     //     // 새로운 중심점
//     //     int newCenterIndex = newRow * SlotConst.FEATURE_COLS + newCol;

//     //     // 영역 업데이트
//     //     area.Clear();
//     //     CalculateAreaAndCenter(area, newCenterIndex, blockSize);

//     //     // 아이템 포함 여부 확인
//     //     bool collected = IsItemInArea(area, itemIndex);

//     //     return (newCenterIndex, collected);
//     // }


//     // /// <summary>
//     // /// 주어진 위치가 현재 영역 안에 있는지 확인
//     // /// </summary>
//     // [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     // private static bool IsItemInArea(Span<byte> area, int itemIndex)
//     // {
//     //     return area[itemIndex] == 1;
//     // }

//     // [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     // public static int FindNearestItemOutsideArea(
//     // Span<byte> area,
//     // int centerIndex,
//     // ReadOnlySpan<byte> itemIndices)
//     // {
//     //     // 현재 위치를 2D 좌표로 변환
//     //     int centerRow = (int)((uint)centerIndex / SlotConst.FEATURE_COLS);
//     //     int centerCol = centerIndex - (centerRow * SlotConst.FEATURE_COLS);

//     //     int nearestIndex = -1;
//     //     int minDistance = int.MaxValue;

//     //     foreach (byte itemIndex in itemIndices)
//     //     {
//     //         // 이미 영역 안에 있는 아이템은 건너뜀
//     //         if (area[itemIndex] == 1)
//     //             continue;

//     //         // 아이템 위치를 2D 좌표로 변환
//     //         int itemRow = (int)((uint)itemIndex / SlotConst.FEATURE_COLS);
//     //         int itemCol = itemIndex - (itemRow * SlotConst.FEATURE_COLS);

//     //         // 맨해튼 거리 계산 (대각선 이동 가능할 경우 체비쇼프 거리 사용)
//     //         int distance = Math.Abs(itemRow - centerRow) + Math.Abs(itemCol - centerCol);
//     //         //int distance = Math.Max(Math.Abs(itemRow - centerRow), Math.Abs(itemCol - centerCol)); // 체비쇼프 거리

//     //         if (distance < minDistance)
//     //         {
//     //             minDistance = distance;
//     //             nearestIndex = itemIndex;
//     //         }
//     //     }

//     //     return nearestIndex;
//     // }

//     // // 사용 예시:
//     // public static void CollectItemsOptimized(Span<byte> area, int centerIndex, int blockSize, ReadOnlySpan<byte> itemIndices)
//     // {
//     //     while (true)
//     //     {
//     //         // 가장 가까운 미수집 아이템 찾기
//     //         int nearestItem = FindNearestItemOutsideArea(area, centerIndex, itemIndices);
//     //         if (nearestItem == -1) // 모든 아이템 수집 완료
//     //             break;

//     //         // 아이템 방향으로 이동하며 수집
//     //         bool collected = false;
//     //         while (!collected)
//     //         {
//     //             (centerIndex, collected) = MoveAreaToItem(area, centerIndex, nearestItem, blockSize);
//     //         }

//     //         Console.WriteLine($"Collected item at position {nearestItem}");
//     //     }
//     // }

//     // /// <summary>
//     // /// 현재 영역과 아이템 간의 거리 계산 (디버깅/시각화용)
//     // /// </summary>
//     // public static void VisualizeDistances(
//     //     int centerIndex,
//     //     ReadOnlySpan<int> itemIndices)
//     // {
//     //     int centerRow = (int)((uint)centerIndex / Const.FEATURE_COLS);
//     //     int centerCol = centerIndex - (centerRow * Const.FEATURE_COLS);

//     //     Console.WriteLine($"\nDistances from center ({centerRow}, {centerCol}):");
//     //     foreach (int itemIndex in itemIndices)
//     //     {
//     //         int itemRow = (int)((uint)itemIndex / Const.FEATURE_COLS);
//     //         int itemCol = itemIndex - (itemRow * Const.FEATURE_COLS);

//     //         int manhattan = Math.Abs(itemRow - centerRow) + Math.Abs(itemCol - centerCol);
//     //         int chebyshev = Math.Max(Math.Abs(itemRow - centerRow), Math.Abs(itemCol - centerCol));

//     //         Console.WriteLine($"Item at ({itemRow}, {itemCol}): " +
//     //                         $"Manhattan = {manhattan}, Chebyshev = {chebyshev}");
//     //     }
//     // }
// }
