
using LottaCashMummy.Buffer;
using LottaCashMummy.Common;
using LottaCashMummy.Statistics.Model;
using System.Diagnostics;


namespace LottaCashMummy.Tests
{
    public class FeatureSymbolValueTest
    {
        private readonly IFeatureData fd;
        private readonly Random random;
        private const int TOTAL_ITERATIONS = 100_000_000;

        public FeatureSymbolValueTest(IFeatureData featureData)
        {
            fd = featureData;
            random = new Random(42); // 고정된 시드 값으로 재현 가능한 결과 생성
        }

        public void RunTest()
        {
            int spinCount = 0;
            int gemCount = 0;

            for (int t = 0; t < TOTAL_ITERATIONS; t++)
            {
                spinCount++;
                for (int i = 0; i < 5; i++)
                {
                    for (int idx = 0; idx < 4; idx++)
                    {
                        // 5% chance to create a gem
                        if (random.Next(1, 101) <= 5)
                        {
                            gemCount++;
                        }
                    }
                }
            }

            var totalSpinCount = TOTAL_ITERATIONS * 5 * 4;
            Console.WriteLine($"spinCount: {spinCount:N}, gemCount: {gemCount:N}, gemRate: {gemCount / (double)totalSpinCount:F2}");
        }
    }
}