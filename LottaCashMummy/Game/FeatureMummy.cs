using System.Diagnostics;
using LottaCashMummy.Buffer;
using LottaCashMummy.Shared;

namespace LottaCashMummy.Game
{
    public class FeatureMummy
    {
        private readonly IFeatureData featureData;

        public FeatureMummy(IFeatureData featureData)
        {
            this.featureData = featureData;
        }

        public void Init(FeatureStorage fs, int mummyPosition)
        {
            if (!featureData.Mummy.TryGetMummyLevel(1, out var mummyLevel))
            {
                throw new Exception("Mummy level not found");
            }

            fs.InitMummy(mummyPosition, mummyLevel!.Area, mummyLevel.Level, mummyLevel.ReqGem);
            CalculateArea(fs, mummyPosition, mummyLevel.Area);
        }

        public bool LevelUp(FeatureStorage fs)
        {
            var spinResult = fs.SpinResult;

            if (spinResult.GemCount <= 0)
                return false;


            var isLevelUp = false;
            var remainGemCount = spinResult.GemCount + spinResult.RemainGemCount;
            while (!fs.Mummy.IsMaxLevel())
            {
                var requiredGemCount = fs.Mummy.GemsToLevel - fs.Mummy.GemCount;
                if (remainGemCount < requiredGemCount)
                {
                    break;
                }

                fs.Mummy.ObtainGem(requiredGemCount);
                remainGemCount -= requiredGemCount;

                if (!fs.Mummy.CanLevelUp())
                {
                    break;
                }

                if (!featureData.Mummy.TryGetMummyLevel(fs.Mummy.Level + 1, out var nextLevel) || nextLevel == null)
                {
                    throw new Exception("Mummy level not found");
                }

                if (!fs.MummyLevelUp(nextLevel.Area, nextLevel.ReqGem, nextLevel.Spin))
                {
                    throw new Exception("Mummy level up failed");
                }

                isLevelUp = true;
            }

            if (remainGemCount < 0)
            {
                throw new Exception("Remain gem count is less than 0");
            }

            spinResult.SetRemainGemCount(remainGemCount);

            return isLevelUp;
        }

        public static int CalculateCenterIndex(int position, int blockSize)
        {
            int row = position / SlotConst.FEATURE_COLS;
            int col = position % SlotConst.FEATURE_COLS;

            // 블록 크기에 따른 사이드 길이 계산
            int sideLength = (int)Math.Sqrt(blockSize);
            int offset = (sideLength - 1) >> 1;

            // 영역 경계를 고려하여 중심 위치 조정
            int centerRow = Math.Clamp(row, offset, SlotConst.FEATURE_ROWS - offset - 1);
            int centerCol = Math.Clamp(col, offset, SlotConst.FEATURE_COLS - offset - 1);

            // 최종 중심 인덱스 계산
            return (centerRow * SlotConst.FEATURE_COLS) + centerCol;
        }

        public static void CalculateArea(FeatureStorage fs, int centerIndex, int blockSize)
        {
            fs.ClearMummyArea();

            int currentRow = centerIndex / SlotConst.FEATURE_COLS;
            int currentCol = centerIndex % SlotConst.FEATURE_COLS;

            int sideLength = (int)Math.Sqrt(blockSize);
            int offset = (sideLength - 1) >> 1;

            int startRow = Math.Max(0, Math.Min(SlotConst.FEATURE_ROWS - sideLength, currentRow - offset));
            int startCol = Math.Max(0, Math.Min(SlotConst.FEATURE_COLS - sideLength, currentCol - offset));

            for (int row = 0; row < sideLength; row++)
            {
                for (int col = 0; col < sideLength; col++)
                {
                    int index = (startRow + row) * SlotConst.FEATURE_COLS + (startCol + col);
                    fs.AddMummyActiveArea(index);

                }
            }
        }
    }
}
